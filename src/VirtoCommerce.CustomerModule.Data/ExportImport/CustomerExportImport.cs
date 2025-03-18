using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VirtoCommerce.CustomerModule.Core;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Model.Search;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CustomerModule.Data.ExportImport
{
    public sealed class CustomerExportImport
    {
        private readonly IMemberService _memberService;
        private readonly IMemberSearchService _memberSearchService;
        private readonly ISettingsManager _settingsManager;
        private readonly JsonSerializer _serializer;

        private int? _batchSize;

        public CustomerExportImport(IMemberService memberService, IMemberSearchService memberSearchService, ISettingsManager settingsManager, JsonSerializer jsonSerializer)
        {
            _memberService = memberService;
            _memberSearchService = memberSearchService;
            _settingsManager = settingsManager;
            _serializer = jsonSerializer;
        }

        public async Task ExportAsync(Stream outStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var progressInfo = new ExportImportProgressInfo { Description = "loading data..." };
            progressCallback(progressInfo);

            var batchSize = await GetBatchSize();

            using (var sw = new StreamWriter(outStream, Encoding.UTF8))
            using (var writer = new JsonTextWriter(sw))
            {
                await writer.WriteStartObjectAsync();

                progressInfo.Description = "Members exporting...";
                progressCallback(progressInfo);

                var members = await _memberSearchService.SearchMembersAsync(new MembersSearchCriteria { Take = 0, DeepSearch = true });
                var memberCount = members.TotalCount;
                await writer.WritePropertyNameAsync("MembersTotalCount");
                await writer.WriteValueAsync(memberCount);

                cancellationToken.ThrowIfCancellationRequested();

                await writer.WritePropertyNameAsync("Members");
                await writer.WriteStartArrayAsync();

                for (var i = 0; i < memberCount; i += batchSize)
                {
                    var searchResponse = await _memberSearchService.SearchMembersAsync(new MembersSearchCriteria { Skip = i, Take = batchSize, DeepSearch = true });
                    foreach (var member in searchResponse.Results)
                    {
                        _serializer.Serialize(writer, member);
                    }
                    await writer.FlushAsync();
                    progressInfo.Description = $"{Math.Min(memberCount, i + batchSize)} of {memberCount} members exported";
                    progressCallback(progressInfo);
                }
                await writer.WriteEndArrayAsync();

                await writer.WriteEndObjectAsync();
                await writer.FlushAsync();
            }
        }

        public async Task ImportAsync(Stream inputStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var progressInfo = new ExportImportProgressInfo();
            var membersTotalCount = 0;

            var batchSize = await GetBatchSize();

            using (var streamReader = new StreamReader(inputStream))
            using (var reader = new JsonTextReader(streamReader))
            {
                while (await reader.ReadAsync())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        var readerValueString = reader.Value?.ToString();

                        if (readerValueString.EqualsIgnoreCase("MembersTotalCount"))
                        {
                            membersTotalCount = await reader.ReadAsInt32Async() ?? 0;
                        }
                        else if (readerValueString.EqualsIgnoreCase("Members"))
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            await reader.ReadAsync();
                            if (reader.TokenType == JsonToken.StartArray)
                            {
                                await reader.ReadAsync();

                                var members = new List<Member>();
                                var membersCount = 0;
                                //TODO: implement to iterative import without whole members loading
                                while (reader.TokenType != JsonToken.EndArray)
                                {
                                    var member = _serializer.Deserialize<Member>(reader);
                                    members.Add(member);
                                    membersCount++;

                                    await reader.ReadAsync();
                                }

                                cancellationToken.ThrowIfCancellationRequested();
                                //Need to import by topological sort order, because Organizations have a graph structure and here references integrity must be preserved 
                                var organizations = members.OfType<Organization>().ToList();
                                var nodes = new HashSet<string>(organizations.Select(x => x.Id));
                                var edges = new HashSet<Tuple<string, string>>(organizations.Where(x => !string.IsNullOrEmpty(x.ParentId) && x.Id != x.ParentId).Select(x => new Tuple<string, string>(x.Id, x.ParentId)));
                                var topologicalSortedList = TopologicalSort.Sort(nodes, edges);
                                members = members.OrderByDescending(x => topologicalSortedList.IndexOf(x.Id)).ToList();

                                for (var i = 0; i < membersCount; i += batchSize)
                                {
                                    await _memberService.SaveChangesAsync(members.Skip(i).Take(batchSize).ToArray());

                                    progressInfo.Description = membersTotalCount > 0
                                        ? $"{i} of {membersTotalCount} members imported"
                                        : $"{i} members imported";

                                    progressCallback(progressInfo);
                                }
                            }
                        }
                    }
                }
            }
        }


        private async Task<int> GetBatchSize()
        {
            _batchSize ??= await _settingsManager.GetValueAsync<int>(ModuleConstants.Settings.General.ExportImportPageSize);

            return (int)_batchSize;
        }
    }
}

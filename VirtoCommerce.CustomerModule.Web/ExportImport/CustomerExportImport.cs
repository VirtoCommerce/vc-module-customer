using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Domain.Customer.Model.Search;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Platform.Core.ExportImport;

namespace VirtoCommerce.CustomerModule.Web.ExportImport
{
    public sealed class CustomerExportImport
    {
        private readonly IMemberService _memberService;
        private readonly IMemberSearchService _memberSearchService;
        private readonly JsonSerializer _serializer;
        private const int _batchSize = 50;

        public CustomerExportImport(IMemberService memberService, IMemberSearchService memberSearchService)
        {
            _memberService = memberService;
            _memberSearchService = memberSearchService;
            _serializer = new JsonSerializer { TypeNameHandling = TypeNameHandling.All };
        }

        public void DoExport(Stream backupStream, Action<ExportImportProgressInfo> progressCallback)
        {
            var progressInfo = new ExportImportProgressInfo { Description = "loading data..." };
            progressCallback(progressInfo);

            using (var sw = new StreamWriter(backupStream, Encoding.UTF8))
            using (var writer = new JsonTextWriter(sw))
            {
                writer.WriteStartObject();

                progressInfo.Description = "Members exporting...";
                progressCallback(progressInfo);

                var memberCount = _memberSearchService.SearchMembers(new MemberSearchCriteria { Take = 0, DeepSearch = true }).TotalCount;
                writer.WritePropertyName("MembersTotalCount");
                writer.WriteValue(memberCount);

                writer.WritePropertyName("Members");
                writer.WriteStartArray();
                for (var i = 0; i < memberCount; i += _batchSize)
                {
                    var searchResponse = _memberSearchService.SearchMembers(new MemberSearchCriteria { Skip = i, Take = _batchSize, DeepSearch = true });
                    foreach (var member in searchResponse.Results)
                    {
                        _serializer.Serialize(writer, member);
                    }
                    writer.Flush();
                    progressInfo.Description = $"{ Math.Min(memberCount, i + _batchSize) } of { memberCount } members exported";
                    progressCallback(progressInfo);
                }
                writer.WriteEndArray();

                writer.WriteEndObject();
                writer.Flush();
            }
        }

        public void DoImport(Stream backupStream, Action<ExportImportProgressInfo> progressCallback)
        {
            var progressInfo = new ExportImportProgressInfo();
            var membersTotalCount = 0;

            using (var streamReader = new StreamReader(backupStream))
            using (var reader = new JsonTextReader(streamReader))
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        if (reader.Value.ToString() == "MembersTotalCount")
                        {
                            membersTotalCount = reader.ReadAsInt32() ?? 0;
                        }
                        else if (reader.Value.ToString() == "Members")
                        {
                            reader.Read();
                            if (reader.TokenType == JsonToken.StartArray)
                            {
                                reader.Read();

                                var members = new List<Member>();
                                var membersCount = 0;

                                while (reader.TokenType != JsonToken.EndArray)
                                {
                                    var member = _serializer.Deserialize<Member>(reader);
                                    members.Add(member);
                                    membersCount++;

                                    reader.Read();
                                }

                                if (membersCount % _batchSize == 0 || reader.TokenType == JsonToken.EndArray)
                                {
                                    _memberService.SaveChanges(members.ToArray());
                                    members.Clear();

                                    if (membersTotalCount > 0)
                                    {
                                        progressInfo.Description = $"{ membersCount } of { membersTotalCount } members imported";
                                    }
                                    else
                                    {
                                        progressInfo.Description = $"{ membersCount } members imported";
                                    }

                                    progressCallback(progressInfo);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
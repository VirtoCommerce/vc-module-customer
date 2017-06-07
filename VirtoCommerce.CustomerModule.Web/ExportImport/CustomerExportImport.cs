using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;

namespace VirtoCommerce.CustomerModule.Web.ExportImport
{
    public sealed class BackupObject
    {
        public Member[] Members { get; set; }
    }

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
            _serializer = new JsonSerializer();
        }

        public void DoExport(Stream backupStream, Action<ExportImportProgressInfo> progressCallback)
        {
            var progressInfo = new ExportImportProgressInfo { Description = "loading data..." };
            progressCallback(progressInfo);

            using (StreamWriter sw = new StreamWriter(backupStream, Encoding.UTF8))
            using (JsonTextWriter writer = new JsonTextWriter(sw))
            {
                writer.WriteStartObject();

                progressInfo.Description = string.Format("Members exporting...");
                progressCallback(progressInfo);

                var memberCount = _memberSearchService.SearchMembers(new MembersSearchCriteria { Take = 0 }).TotalCount;
                writer.WritePropertyName("MembersTotalCount");
                writer.WriteValue(memberCount);

                writer.WritePropertyName("Members");
                writer.WriteStartArray();
                for (var i = 0; i < memberCount; i += _batchSize)
                {
                    var searchResponse = _memberSearchService.SearchMembers(new MembersSearchCriteria { Skip = i, Take = _batchSize, DeepSearch = true });
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
            var backupObject = backupStream.DeserializeJson<BackupObject>();
            var originalObject = GetBackupObject(progressCallback);

            var progressInfo = new ExportImportProgressInfo();
            progressInfo.Description = String.Format("{0} members importing...", backupObject.Members.Count());
            progressCallback(progressInfo);
            _memberService.SaveChanges(backupObject.Members.OrderByDescending(x => x.MemberType).ToArray());

        }

        #region BackupObject

        public BackupObject GetBackupObject(Action<ExportImportProgressInfo> progressCallback)
        {
            var progressInfo = new ExportImportProgressInfo();
            progressInfo.Description = "loading members...";
            progressCallback(progressInfo);

            var memberCount = _memberSearchService.SearchMembers(new MembersSearchCriteria { Take = 0 }).TotalCount;
            for (var i = 0; i < memberCount; i += _batchSize)
            {
                var searchResponse = _memberSearchService.SearchMembers(new MembersSearchCriteria { DeepSearch = true, Skip = i, Take = _batchSize });

            }

            var members = _memberSearchService.SearchMembers(new MembersSearchCriteria { DeepSearch = true, Take = int.MaxValue }).Results;

            var result = new BackupObject();
            result.Members = _memberService.GetByIds(members.Select(x => x.Id).ToArray()).OrderByDescending(x => x.MemberType).ToArray();

            return result;
        }

        #endregion

    }
}
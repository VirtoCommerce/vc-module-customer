using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CustomerModule.Data.Handlers
{
    public class IndexMemberChangedEventHandler : IEventHandler<MemberChangedEvent>
    {
        private readonly IIndexingManager _indexingManager;

        public IndexMemberChangedEventHandler(IIndexingManager indexingManager)
        {
            _indexingManager = indexingManager;
        }

        public Task Handle(MemberChangedEvent message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var indexMemberIds = message.ChangedEntries.Where(x => (x.EntryState == EntryState.Modified || x.EntryState == EntryState.Added) && x.OldEntry.Id != null)
                                                          .Select(x => x.OldEntry.Id)
                                                          .Distinct().ToArray();

            if (!indexMemberIds.IsNullOrEmpty())
            {
                BackgroundJob.Enqueue(() => TryIndexMemberBackgroundJob(indexMemberIds));
            }

            var delereIndexMemberIds = message.ChangedEntries.Where(x => x.EntryState == EntryState.Deleted && x.OldEntry.Id != null)
                                                          .Select(x => x.OldEntry.Id)
                                                          .Distinct().ToArray();

            if (!delereIndexMemberIds.IsNullOrEmpty())
            {
                BackgroundJob.Enqueue(() => TryDeleteIndexMemberBackgroundJob(delereIndexMemberIds));
            }

            return Task.CompletedTask;
        }

        [DisableConcurrentExecution(60 * 60 * 24)]
        public async Task TryIndexMemberBackgroundJob(string[] indexMemberIds)
        {
            await TryIndexMember(indexMemberIds);
        }

        [DisableConcurrentExecution(60 * 60 * 24)]
        public async Task TryDeleteIndexMemberBackgroundJob(string[] indexMemberIds)
        {
            await TryDeleteIndexMember(indexMemberIds);
        }


        protected virtual async Task TryIndexMember(string[] indexMemberIds)
        {
            await _indexingManager.IndexDocumentsAsync(KnownDocumentTypes.Member, indexMemberIds);
        }

        protected virtual async Task TryDeleteIndexMember(string[] indexMemberIds)
        {
            await _indexingManager.DeleteDocumentsAsync(KnownDocumentTypes.Member, indexMemberIds);
        }
    }
}

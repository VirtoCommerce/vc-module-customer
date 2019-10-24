using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.Domain.Customer.Events;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

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

            var deletedIndexMemberIds = message.ChangedEntries.Where(x => x.EntryState == EntryState.Deleted && x.OldEntry.Id != null)
                                                          .Select(x => x.OldEntry.Id)
                                                          .Distinct().ToArray();

            if (!deletedIndexMemberIds.IsNullOrEmpty())
            {
                BackgroundJob.Enqueue(() => TryDeleteIndexMemberBackgroundJob(deletedIndexMemberIds));
            }

            return Task.CompletedTask;
        }

        [DisableConcurrentExecution(60 * 60 * 24)]
        public Task TryIndexMemberBackgroundJob(string[] indexMemberIds)
        {
            return TryIndexMember(indexMemberIds);
        }

        [DisableConcurrentExecution(60 * 60 * 24)]
        public Task TryDeleteIndexMemberBackgroundJob(string[] deletedIndexMemberIds)
        {
            return TryDeleteIndexMember(deletedIndexMemberIds);
        }


        protected virtual Task TryIndexMember(string[] indexMemberIds)
        {
            return _indexingManager.IndexDocumentsAsync(KnownDocumentTypes.Member, indexMemberIds);
        }

        protected virtual Task TryDeleteIndexMember(string[] deletedIndexMemberIds)
        {
            return _indexingManager.DeleteDocumentsAsync(KnownDocumentTypes.Member, deletedIndexMemberIds);
        }
    }
}

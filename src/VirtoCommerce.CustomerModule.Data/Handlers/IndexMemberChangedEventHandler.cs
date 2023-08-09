using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.CustomerModule.Data.Search.Indexing;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Security.Events;
using VirtoCommerce.SearchModule.Core.BackgroundJobs;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CustomerModule.Data.Handlers
{
    public class IndexMemberChangedEventHandler : IEventHandler<MemberChangedEvent>, IEventHandler<UserChangedEvent>, IEventHandler<UserRoleAddedEvent>, IEventHandler<UserRoleRemovedEvent>
    {
        private readonly IIndexingJobService _indexingJobService;
        private readonly IEnumerable<IndexDocumentConfiguration> _configurations;

        public IndexMemberChangedEventHandler(IIndexingJobService indexingJobService, IEnumerable<IndexDocumentConfiguration> configurations)
        {
            _configurations = configurations;
            _indexingJobService = indexingJobService;
        }

        public Task Handle(MemberChangedEvent message)
        {
            return InnerHandle(message);
        }

        public Task Handle(UserChangedEvent message)
        {
            return InnerHandle(message);
        }

        public Task Handle(UserRoleAddedEvent message)
        {
            return InnerHandle(message);
        }

        public Task Handle(UserRoleRemovedEvent message)
        {
            return InnerHandle(message);
        }

        protected virtual Task InnerHandle(DomainEvent message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var indexEntries = new List<IndexEntry>();

            switch (message)
            {
                case MemberChangedEvent memberChangedEvent:
                    indexEntries = memberChangedEvent.ChangedEntries
                        .Select(x => new IndexEntry { Id = x.OldEntry.Id, EntryState = x.EntryState, Type = KnownDocumentTypes.Member })
                        .ToList();
                    break;
                case UserChangedEvent userChangedEvent:
                    indexEntries = userChangedEvent.ChangedEntries
                        .Where(x => !string.IsNullOrEmpty(x.OldEntry.MemberId))
                        .Select(x => new IndexEntry { Id = x.OldEntry.MemberId, EntryState = x.EntryState, Type = KnownDocumentTypes.Member })
                        .ToList();
                    break;
                case UserRoleAddedEvent userRoleAddedEvent when !string.IsNullOrEmpty(userRoleAddedEvent.User.MemberId):
                    indexEntries.Add(GetIndexEntry(userRoleAddedEvent.User.MemberId));
                    break;
                case UserRoleRemovedEvent userRoleRemovedEvent when !string.IsNullOrEmpty(userRoleRemovedEvent.User.MemberId):
                    indexEntries.Add(GetIndexEntry(userRoleRemovedEvent.User.MemberId));
                    break;
            }

            _indexingJobService.EnqueueIndexAndDeleteDocuments(indexEntries, JobPriority.Normal,
                _configurations.GetDocumentBuilders(KnownDocumentTypes.Member, typeof(MemberDocumentChangesProvider)).ToList());

            return Task.CompletedTask;
        }

        protected virtual IndexEntry GetIndexEntry(string objectId)
        {
            var result = new IndexEntry
            {
                Id = objectId,
                EntryState = EntryState.Modified,
                Type = KnownDocumentTypes.Member,
            };

            return result;
        }
    }
}

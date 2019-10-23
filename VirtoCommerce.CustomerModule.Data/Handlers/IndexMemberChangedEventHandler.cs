using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CoreModule.Data.Indexing.BackgroundJobs;
using VirtoCommerce.Domain.Customer.Events;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CustomerModule.Data.Handlers
{
    public class IndexMemberChangedEventHandler : IEventHandler<MemberChangedEvent>
    {
        public Task Handle(MemberChangedEvent message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var indexEntries = message.ChangedEntries
                .Select(x => new IndexEntry { Id = x.OldEntry.Id, EntryState = x.EntryState, Type = KnownDocumentTypes.Member })
                .ToArray();

            IndexingJobs.EnqueueIndexAndDeleteDocuments(indexEntries);

            return Task.CompletedTask;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.CustomerModule.Data.Search.Indexing;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Data.BackgroundJobs;
using VirtoCommerce.SearchModule.Data.Services;

namespace VirtoCommerce.CustomerModule.Data.Handlers
{
    public class IndexMemberChangedEventHandler : IEventHandler<MemberChangedEvent>
    {
        private readonly IEnumerable<IndexDocumentConfiguration> _configurations;

        public IndexMemberChangedEventHandler(IEnumerable<IndexDocumentConfiguration> configurations)
        {
            _configurations = configurations;
        }

        public Task Handle(MemberChangedEvent message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var indexEntries = message.ChangedEntries
                .Select(x => new IndexEntry { Id = x.OldEntry.Id, EntryState = x.EntryState, Type = KnownDocumentTypes.Member })
                .ToArray();

            IndexingJobs.EnqueueIndexAndDeleteDocuments(indexEntries,
                JobPriority.Normal, _configurations.GetBuildersForProvider(typeof(MemberDocumentChangesProvider)).ToList());

            return Task.CompletedTask;
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Data.BackgroundJobs;

namespace VirtoCommerce.CustomerModule.Data.Handlers
{
    public class IndexMemberChangedEventHandler : IEventHandler<MemberChangedEvent>
    {
        private readonly ISettingsManager _settingsManager;

        public IndexMemberChangedEventHandler(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        public Task Handle(MemberChangedEvent message)
        {
            if (_settingsManager.GetValue(Core.ModuleConstants.Settings.General.EventBasedIndexation.Name, false))
            {
                if (message == null)
                {
                    throw new ArgumentNullException(nameof(message));
                }

                var indexEntries = message.ChangedEntries
                    .Select(x => new IndexEntry { Id = x.OldEntry.Id, EntryState = x.EntryState, Type = KnownDocumentTypes.Member })
                    .ToArray();

                IndexingJobs.EnqueueIndexAndDeleteDocuments(indexEntries);
            }

            return Task.CompletedTask;
        }
    }
}

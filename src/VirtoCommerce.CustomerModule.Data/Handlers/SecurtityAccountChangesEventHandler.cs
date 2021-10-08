using System;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Data.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Security.Events;

namespace VirtoCommerce.CustomerModule.Data.Handlers
{
    public class SecurtityAccountChangesEventHandler : IEventHandler<UserChangedEvent>
    {
        public Task Handle(UserChangedEvent @event)
        {
            InnerHandle(@event);
            return Task.CompletedTask;
        }

        protected virtual void InnerHandle(UserChangedEvent @event)
        {
            foreach (var change in @event.ChangedEntries ?? Array.Empty<GenericChangedEntry<ApplicationUser>>())
            {
                CustomerSearchCacheRegion.ExpireRegion();
                CustomerCacheRegion.ExpireMemberById(change.NewEntry.MemberId);

                if (!change.NewEntry.MemberId.EqualsInvariant(change.OldEntry.MemberId))
                {
                    CustomerCacheRegion.ExpireMemberById(change.OldEntry.MemberId);
                }
            }
        }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CustomerModule.Data.Handlers
{
    public class RevokeTokenOrganizationMembershipChangedEventHandler(
        Func<(IUserSessionsService SessionService, IServiceScope Scope)> userSessionsServiceFactory)
        : IEventHandler<OrganizationMembershipChangedEvent>
    {
        public virtual async Task Handle(OrganizationMembershipChangedEvent message)
        {
            foreach (var changedEntry in message.ChangedEntries)
            {
                if (changedEntry.EntryState != EntryState.Added && changedEntry.EntryState != EntryState.Modified)
                {
                    continue;
                }

                if (changedEntry.NewEntry.IsCurrentlyLocked)
                {
                    await RevokeUserTokensAsync(changedEntry.NewEntry.UserId);
                }
            }
        }

        protected virtual async Task RevokeUserTokensAsync(string userId)
        {
            var (sessionService, scope) = userSessionsServiceFactory();
            using var _ = scope;
            await sessionService.TerminateAllUserSessions(userId);
        }
    }
}

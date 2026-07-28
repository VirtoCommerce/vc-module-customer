using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CustomerModule.Core;
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
                switch (changedEntry.EntryState)
                {
                    case EntryState.Deleted:
                        await RevokeUserTokensAsync(changedEntry.OldEntry.UserId);
                        break;

                    case EntryState.Modified when changedEntry.NewEntry.IsCurrentlyLocked || ModuleConstants.MembershipStatuses.IsBlocking(changedEntry.NewEntry.Status):
                        await RevokeUserTokensAsync(changedEntry.NewEntry.UserId);
                        break;
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

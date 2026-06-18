using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security.Events;

namespace VirtoCommerce.CustomerModule.Data.Handlers
{
    public class DeleteOrganizationMembershipUserChangedEventHandler(
        IOrganizationMembershipService organizationMembershipService)
        : IEventHandler<UserChangedEvent>,
          IEventHandler<MemberChangedEvent>
    {
        public virtual async Task Handle(UserChangedEvent message)
        {
            var userIds = message.ChangedEntries
                ?.Where(e => e.EntryState == EntryState.Deleted)
                .Select(e => e.OldEntry.Id)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            if (userIds is not { Count: > 0 })
            {
                return;
            }

            foreach (var userId in userIds)
            {
                await DeleteMembershipsAsync(userId);
            }
        }

        public virtual async Task Handle(MemberChangedEvent message)
        {
            var userIds = message.ChangedEntries
                ?.Where(e => e.EntryState == EntryState.Deleted)
                .Select(e => e.OldEntry)
                .OfType<IHasSecurityAccounts>()
                .SelectMany(m => m.SecurityAccounts ?? [])
                .Select(u => u.Id)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            if (userIds is not { Count: > 0 })
            {
                return;
            }

            foreach (var userId in userIds)
            {
                await DeleteMembershipsAsync(userId);
            }
        }

        protected virtual async Task DeleteMembershipsAsync(string userId)
        {
            var searchResult = await organizationMembershipService.SearchAsync(
                new OrganizationMembershipSearchCriteria
                {
                    UserId = userId,
                    Take = int.MaxValue
                });

            var ids = searchResult?.Results?
                .Select(m => m.Id)
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();

            if (ids is not { Count: > 0 })
            {
                return;
            }

            await organizationMembershipService.DeleteAsync(ids);
        }
    }
}

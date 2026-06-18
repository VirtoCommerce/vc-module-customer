using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Security.Events;
using VirtoCommerce.Platform.Core.Security.Search;

namespace VirtoCommerce.CustomerModule.Data.Handlers
{
    public class DeleteOrganizationMembershipUserChangedEventHandler(
        IOrganizationMembershipService organizationMembershipService,
        IUserSearchService userSearchService)
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
            var deletedMemberIds = message.ChangedEntries
                ?.Where(e => e.EntryState == EntryState.Deleted)
                .Select(e => e.OldEntry.Id)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            if (deletedMemberIds is { Count: > 0 })
            {
                var userIds = await GetUserIdsByMemberIdsAsync(deletedMemberIds);
                foreach (var userId in userIds)
                {
                    await DeleteMembershipsAsync(userId);
                }
            }

            foreach (var entry in message.ChangedEntries?.Where(e => e.EntryState == EntryState.Modified) ?? [])
            {
                if (entry.OldEntry is not IHasOrganizations oldOrgs ||
                    entry.NewEntry is not IHasOrganizations newOrgs)
                {
                    continue;
                }

                var removedOrgIds = (oldOrgs.Organizations ?? [])
                    .Except(newOrgs.Organizations ?? [], StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (removedOrgIds.Count == 0)
                {
                    continue;
                }

                var userIds = await GetUserIdsByMemberIdsAsync([entry.OldEntry.Id]);
                foreach (var userId in userIds)
                {
                    await DeleteMembershipsForOrgsAsync(userId, removedOrgIds);
                }
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

        protected virtual async Task DeleteMembershipsForOrgsAsync(string userId, IList<string> organizationIds)
        {
            var searchResult = await organizationMembershipService.SearchAsync(
                new OrganizationMembershipSearchCriteria
                {
                    UserId = userId,
                    Take = int.MaxValue
                });

            var orgIdSet = new HashSet<string>(organizationIds, StringComparer.OrdinalIgnoreCase);
            var ids = searchResult?.Results?
                .Where(m => orgIdSet.Contains(m.OrganizationId ?? string.Empty))
                .Select(m => m.Id)
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();

            if (ids is not { Count: > 0 })
            {
                return;
            }

            await organizationMembershipService.DeleteAsync(ids);
        }

        private async Task<IList<string>> GetUserIdsByMemberIdsAsync(IList<string> memberIds)
        {
            var result = await userSearchService.SearchUsersAsync(
                new UserSearchCriteria
                {
                    MemberIds = memberIds,
                    Take = int.MaxValue
                });

            return result?.Results?
                .Select(u => u.Id)
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList() ?? [];
        }
    }
}

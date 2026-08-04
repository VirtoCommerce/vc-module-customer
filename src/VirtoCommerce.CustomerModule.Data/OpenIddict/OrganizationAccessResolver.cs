using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Data.OpenIddict;

internal static class OrganizationAccessResolver
{
    public static async Task<string> ResolveOrganizationIdAsync(
        IOrganizationMembershipSearchService organizationMembershipSearchService,
        string userId,
        Member member)
    {
        if (member is not IHasOrganizations contact)
        {
            return null;
        }

        var organizations = contact.Organizations ?? [];
        var accessibleOrganizations = await GetAccessibleOrganizationIdsAsync(organizationMembershipSearchService, userId, member, organizations);

        if (!contact.CurrentOrganizationId.IsNullOrEmpty() &&
            accessibleOrganizations.Contains(contact.CurrentOrganizationId, StringComparer.OrdinalIgnoreCase))
        {
            return contact.CurrentOrganizationId;
        }

        if (!contact.DefaultOrganizationId.IsNullOrEmpty() &&
            accessibleOrganizations.Contains(contact.DefaultOrganizationId, StringComparer.OrdinalIgnoreCase))
        {
            return contact.DefaultOrganizationId;
        }

        return accessibleOrganizations.FirstOrDefault();
    }

    public static async Task<IReadOnlyCollection<string>> GetAccessibleOrganizationIdsAsync(
        IOrganizationMembershipSearchService organizationMembershipSearchService,
        string userId,
        Member member,
        IList<string> organizationIds)
    {
        if (string.IsNullOrEmpty(userId) || organizationIds.IsNullOrEmpty())
        {
            return organizationIds?.ToList() ?? [];
        }

        var memberships = await organizationMembershipSearchService.SearchAllNoCloneAsync(new OrganizationMembershipSearchCriteria
        {
            UserId = userId,
            OrganizationIds = organizationIds,
        });

        var membershipByOrgId = memberships
            .Where(m => m.OrganizationId != null)
            .GroupBy(m => m.OrganizationId)
            .ToDictionary(g => g.Key, g => g.First());

        return organizationIds
            .Where(orgId =>
            {
                membershipByOrgId.TryGetValue(orgId, out var membership);

                if (membership?.IsCurrentlyLocked == true)
                {
                    return false;
                }

                var effectiveStatus = OrganizationMembership.ResolveEffectiveStatus(membership?.Status, member?.Status);

                return !ModuleConstants.MembershipStatuses.IsBlocking(effectiveStatus);
            })
            .ToList();
    }
}

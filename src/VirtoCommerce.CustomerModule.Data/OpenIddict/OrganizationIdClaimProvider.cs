using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.Extensions;
using VirtoCommerce.Platform.Security.OpenIddict;
using static VirtoCommerce.CustomerModule.Core.ModuleConstants.Security;

namespace VirtoCommerce.CustomerModule.Data.OpenIddict;

public class OrganizationIdClaimProvider(
    IMemberService memberService,
    IOrganizationMembershipService organizationMembershipService,
    Func<RoleManager<Role>> roleManagerFactory) : ITokenClaimProvider
{
    public virtual async Task SetClaimsAsync(ClaimsPrincipal principal, TokenRequestContext context)
    {
        var organizationId = await GetOrganizationId(context);
        principal.SetClaimWithDestinations(Claims.OrganizationId, organizationId, [OpenIddictConstants.Destinations.AccessToken]);

        if (!organizationId.IsNullOrEmpty() && context.User != null)
        {
            await AddOrgScopedPermissionsAsync(principal, context.User.Id, context.User.MemberId, organizationId);
        }
    }

    private async Task AddOrgScopedPermissionsAsync(ClaimsPrincipal principal, string userId, string memberId, string organizationId)
    {
        // If user has an explicit membership record and is locked — deny all org-scoped permissions
        var membership = await organizationMembershipService.GetByUserAndOrgAsync(userId, organizationId);
        if (membership?.IsCurrentlyLocked == true)
        {
            return;
        }

        // Without an explicit membership row, verify via the contact's Organizations list to prevent
        // privilege escalation when an arbitrary organizationId is passed in the token request
        if (membership == null && !await IsContactMemberOfOrgAsync(memberId, organizationId))
        {
            return;
        }

        if (principal.Identity is not ClaimsIdentity identity)
        {
            return;
        }

        var allRoleIds = await GetOrgScopedRoleIdsAsync(organizationId, membership);

        if (allRoleIds.Count == 0)
        {
            return;
        }

        // Collect permissions already present in the token (from global roles) to avoid duplicates
        var existingPermissions = principal.Claims
            .Where(c => c.Type == PlatformConstants.Security.Claims.PermissionClaimType)
            .Select(c => c.Value)
            .ToHashSet();

        await AddRolePermissionsAsync(identity, allRoleIds, existingPermissions);
    }

    private async Task<List<string>> GetOrgScopedRoleIdsAsync(string organizationId, OrganizationMembership membership)
    {
        // Organization-level roles apply to all org members regardless of explicit OrganizationMembership record
        var organization = await memberService.GetByIdAsync(organizationId, memberType: nameof(Organization)) as Organization;
        var orgRoleIds = organization?.Roles?.Select(r => r.RoleId) ?? [];

        // Membership-level roles are user-specific within the org (only when membership record exists)
        var membershipRoleIds = membership?.Roles?.Select(r => r.RoleId) ?? [];

        return orgRoleIds.Concat(membershipRoleIds).Distinct().ToList();
    }

    private async Task AddRolePermissionsAsync(ClaimsIdentity identity, IList<string> roleIds, HashSet<string> existingPermissions)
    {
        using var roleManager = roleManagerFactory();
        foreach (var roleId in roleIds)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                continue;
            }

            var roleClaims = await roleManager.GetClaimsAsync(role);
            foreach (var claim in roleClaims)
            {
                if (claim.Type != PlatformConstants.Security.Claims.PermissionClaimType || !existingPermissions.Add(claim.Value))
                {
                    continue;
                }

                identity.AddClaim(
                    new Claim(claim.Type, claim.Value)
                        .SetDestinations(OpenIddictConstants.Destinations.AccessToken));
            }
        }
    }

    private async Task<bool> IsContactMemberOfOrgAsync(string memberId, string organizationId)
    {
        if (string.IsNullOrEmpty(memberId))
        {
            return false;
        }

        var contact = await memberService.GetByIdAsync(memberId) as IHasOrganizations;

        return contact?.Organizations?.ContainsIgnoreCase(organizationId) == true;
    }

    private async Task<string> GetOrganizationId(TokenRequestContext context)
    {
        var organizationId = context.Request.GetParameter(Parameters.OrganizationId)?.ToString();
        if (!string.IsNullOrEmpty(organizationId))
        {
            return organizationId;
        }

        organizationId = context.Principal?.FindFirstValue(Claims.OrganizationId);
        if (!string.IsNullOrEmpty(organizationId))
        {
            return organizationId;
        }

        return await GetMemberOrganizationId(context);
    }

    private async Task<string> GetMemberOrganizationId(TokenRequestContext context)
    {
        var memberId = context.User?.MemberId;
        if (string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        var member = await memberService.GetByIdAsync(memberId);

        return member switch
        {
            IHasOrganizations contact => GetContactOrganizationId(contact),
            _ => null,
        };
    }

    private static string GetContactOrganizationId(IHasOrganizations contact)
    {
        var organizations = contact.Organizations ?? [];

        if (!contact.CurrentOrganizationId.IsNullOrEmpty() && organizations.ContainsIgnoreCase(contact.CurrentOrganizationId))
        {
            return contact.CurrentOrganizationId;
        }

        if (!contact.DefaultOrganizationId.IsNullOrEmpty() && organizations.ContainsIgnoreCase(contact.DefaultOrganizationId))
        {
            return contact.DefaultOrganizationId;
        }

        return organizations.FirstOrDefault();
    }
}

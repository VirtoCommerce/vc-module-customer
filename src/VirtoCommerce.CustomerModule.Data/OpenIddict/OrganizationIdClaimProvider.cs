using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using VirtoCommerce.CustomerModule.Core;
using VirtoCommerce.CustomerModule.Core.Extensions;
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
    IOrganizationMembershipSearchService organizationMembershipSearchService,
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
        if (principal.Identity is not ClaimsIdentity identity)
        {
            return;
        }

        var roleIds = await GetOrgScopedRoleIdsAsync(userId, memberId, organizationId);
        if (roleIds.Count == 0)
        {
            return;
        }

        var existingPermissions = principal.Claims
            .Where(c => c.Type == PlatformConstants.Security.Claims.PermissionClaimType)
            .Select(c => c.Value)
            .ToHashSet();

        await AddRolePermissionsAsync(identity, roleIds, existingPermissions);
    }

    private async Task<IList<string>> GetOrgScopedRoleIdsAsync(string userId, string memberId, string organizationId)
    {
        var membership = await organizationMembershipSearchService.GetMembershipAsync(userId, organizationId);

        if (membership?.IsCurrentlyLocked == true)
        {
            return [];
        }

        var member = string.IsNullOrEmpty(memberId) ? null : await memberService.GetByIdAsync(memberId);

        var effectiveStatus = OrganizationMembership.ResolveEffectiveStatus(membership?.Status, member?.Status);
        if (ModuleConstants.MembershipStatuses.IsBlocking(effectiveStatus))
        {
            return [];
        }

        var isMemberOfOrg = (member as IHasOrganizations)?.Organizations?.ContainsIgnoreCase(organizationId) == true;
        if (membership == null && !isMemberOfOrg)
        {
            return [];
        }

        var orgScopedRoles = await organizationMembershipSearchService.GetRolesByUserAndOrgAsync(organizationId, membership);

        return orgScopedRoles.Select(r => r.RoleId).ToList();
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

        return await OrganizationAccessResolver.ResolveOrganizationIdAsync(organizationMembershipSearchService, context.User?.Id, member);
    }
}

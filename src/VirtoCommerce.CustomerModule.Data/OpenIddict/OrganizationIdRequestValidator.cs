using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using OpenIddict.Abstractions;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.OpenIddict;
using static VirtoCommerce.CustomerModule.Core.ModuleConstants.Security;

namespace VirtoCommerce.CustomerModule.Data.OpenIddict;

public class OrganizationIdRequestValidator(
    IMemberService memberService,
    IOrganizationMembershipService organizationMembershipService) : ITokenRequestValidator
{
    public virtual int Priority { get; set; } = 50;

    public virtual async Task<IList<TokenResponse>> ValidateAsync(TokenRequestContext context)
    {
        var organizationId = await GetOrganizationId(context);
        if (string.IsNullOrEmpty(organizationId))
        {
            return [];
        }

        // Global lock has the highest priority — if the user is globally locked out,
        // return the global error immediately without checking org-level state.
        if (context.User != null && IsGloballyLocked(context.User))
        {
            return [ErrorDescriber.UserIsLockedOut()];
        }

        var availableOrganizationIds = await GetAvailableOrganizationIds(context);
        if (!availableOrganizationIds.Contains(organizationId))
        {
            // Replace invalid organization ID with default organization ID when signing in
            if (context.Request.GrantType == OpenIddictConstants.GrantTypes.Password)
            {
                context.Request.SetParameter(Parameters.OrganizationId, availableOrganizationIds.FirstOrDefault());
                return [];
            }

            return [ErrorDescriber.InvalidOrganizationId(organizationId)];
        }

        if (context.User != null)
        {
            var membership = await organizationMembershipService.GetByUserAndOrgAsync(context.User.Id, organizationId);
            if (membership != null && membership.IsCurrentlyLocked)
            {
                return [ErrorDescriber.UserIsLockedInOrganization(organizationId)];
            }
        }

        return [];
    }

    /// <summary>
    /// Returns true when the platform-level (global) lockout is active for the user.
    /// Global lockout has higher priority than organisation-level lockout.
    /// </summary>
    private static bool IsGloballyLocked(ApplicationUser user)
    {
        return user.LockoutEnabled
            && user.LockoutEnd.HasValue
            && user.LockoutEnd.Value > DateTimeOffset.UtcNow;
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

        if (member is not IHasOrganizations contact)
        {
            return null;
        }

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

    private async Task<IList<string>> GetAvailableOrganizationIds(TokenRequestContext context)
    {
        var memberId = context.User?.MemberId;

        if (string.IsNullOrEmpty(memberId))
        {
            return [];
        }

        var member = await memberService.GetByIdAsync(memberId);

        return member switch
        {
            Contact contact => contact.Organizations ?? [],
            Employee employee => employee.Organizations ?? [],
            _ => [],
        };
    }
}

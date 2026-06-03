using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using OpenIddict.Abstractions;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.OpenIddict;
using static VirtoCommerce.CustomerModule.Core.ModuleConstants.Security;

namespace VirtoCommerce.CustomerModule.Data.OpenIddict;

public class OrganizationIdRequestValidator(
    Func<IMemberService> memberServiceFactory,
    IOrganizationMembershipService organizationMembershipService) : ITokenRequestValidator
{
    public virtual int Priority { get; set; } = 50;

    public virtual async Task<IList<TokenResponse>> ValidateAsync(TokenRequestContext context)
    {
        var organizationId = GetOrganizationId(context);
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

    private static string GetOrganizationId(TokenRequestContext context)
    {
        var organizationId = context.Request.GetParameter(Parameters.OrganizationId)?.ToString();
        if (!string.IsNullOrEmpty(organizationId))
        {
            return organizationId;
        }

        return context.Principal?.FindFirstValue(Claims.OrganizationId);
    }

    private async Task<IList<string>> GetAvailableOrganizationIds(TokenRequestContext context)
    {
        var memberId = context.User?.MemberId;

        if (string.IsNullOrEmpty(memberId))
        {
            return [];
        }

        var member = await memberServiceFactory().GetByIdAsync(memberId);

        return member switch
        {
            Contact contact => contact.Organizations ?? [],
            Employee employee => employee.Organizations ?? [],
            _ => [],
        };
    }
}

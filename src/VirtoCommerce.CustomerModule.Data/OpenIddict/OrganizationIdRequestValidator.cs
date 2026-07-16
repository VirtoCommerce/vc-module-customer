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
    IOrganizationMembershipSearchService organizationMembershipSearchService) : ITokenRequestValidator
{
    public virtual int Priority { get; set; } = 50;

    public virtual async Task<IList<TokenResponse>> ValidateAsync(TokenRequestContext context)
    {
        // Captured before any fallback below rewrites the parameter, so an explicit request for a specific
        // organization can be told apart from one auto-resolved from the member.
        var requestedOrganizationId = context.Request.GetParameter(Parameters.OrganizationId)?.ToString();
        var organizationId = await GetOrganizationId(context);
        if (string.IsNullOrEmpty(organizationId))
        {
            return [];
        }

        // Global lock has the highest priority — if the user is globally locked out,
        // return the global error immediately without checking org-level state.
        if (context.User != null && IsGloballyLocked(context.User))
        {
            return [GetGlobalLockoutError(context.User)];
        }

        var availableOrganizationIds = await GetAvailableOrganizationIds(context);
        if (!availableOrganizationIds.Contains(organizationId))
        {
            return HandleUnavailableOrganization(context, organizationId, availableOrganizationIds);
        }

        if (context.User != null)
        {
            var lockError = await ValidateOrganizationLockAsync(context, requestedOrganizationId, organizationId, availableOrganizationIds);
            if (lockError != null)
            {
                return [lockError];
            }
        }

        return [];
    }

    private static TokenResponse GetGlobalLockoutError(ApplicationUser user)
    {
        // Distinguish a permanent lock (LockoutEnd == DateTime.MaxValue) from a temporary failed-attempt lock,
        // mirroring how the platform's BaseUserSignInValidator decides. Otherwise org members get the
        // permanent-worded code for a self-clearing 15-min lock.
        var permanentLockOut = user.LockoutEnd == DateTime.MaxValue.ToUniversalTime();
        return permanentLockOut
            ? ErrorDescriber.UserIsLockedOut()
            : SecurityErrorDescriber.UserIsTemporaryLockedOut();
    }

    private static IList<TokenResponse> HandleUnavailableOrganization(TokenRequestContext context, string organizationId, IList<string> availableOrganizationIds)
    {
        // Replace an invalid organization ID with the default organization ID when signing in.
        if (context.Request.GrantType == OpenIddictConstants.GrantTypes.Password)
        {
            context.Request.SetParameter(Parameters.OrganizationId, availableOrganizationIds.FirstOrDefault());
            return [];
        }

        return [ErrorDescriber.InvalidOrganizationId(organizationId)];
    }

    private async Task<TokenResponse> ValidateOrganizationLockAsync(TokenRequestContext context, string requestedOrganizationId, string organizationId, IList<string> availableOrganizationIds)
    {
        var membership = (await organizationMembershipSearchService.SearchAsync(new OrganizationMembershipSearchCriteria
        {
            UserId = context.User.Id,
            OrganizationId = organizationId,
            Take = 1,
        })).Results.FirstOrDefault();

        if (membership is null || !membership.IsCurrentlyLocked)
        {
            return null;
        }

        // When the organization was auto-resolved (the caller didn't request a specific one) on a fresh password
        // sign-in, a lock in that single organization must not lock the user out of the others they belong to —
        // e.g. a sales rep serving many organizations. Fall back to a non-locked organization instead. Block only
        // when the caller explicitly asked for this (now-locked) organization, or every organization is locked.
        var isAutoResolved = string.IsNullOrEmpty(requestedOrganizationId) &&
            context.Request.GrantType == OpenIddictConstants.GrantTypes.Password;
        if (isAutoResolved)
        {
            var unlockedOrganizationId = await GetFirstUnlockedOrganizationId(context.User.Id, availableOrganizationIds);
            if (!string.IsNullOrEmpty(unlockedOrganizationId))
            {
                context.Request.SetParameter(Parameters.OrganizationId, unlockedOrganizationId);
                return null;
            }
        }

        return ErrorDescriber.UserIsLockedInOrganization(organizationId);
    }

    private async Task<string> GetFirstUnlockedOrganizationId(string userId, IList<string> availableOrganizationIds)
    {
        var lockedOrganizationIds = (await organizationMembershipSearchService.GetLockedOrganizationIdsAsync(userId))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return availableOrganizationIds.FirstOrDefault(id => !lockedOrganizationIds.Contains(id));
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

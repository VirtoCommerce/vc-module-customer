using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using OpenIddict.Abstractions;
using VirtoCommerce.CustomerModule.Core;
using VirtoCommerce.CustomerModule.Core.Extensions;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
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
        // The storefront always resends the last-used organization on every password login (useAuth.ts),
        // so a blocked org here is never a deliberate choice — allow falling back. An explicit org switch
        // on an existing session (refresh_token grant, switchOrganization()) gets no fallback.
        var allowFallback = context.Request.GrantType == OpenIddictConstants.GrantTypes.Password;

        var member = await GetMemberAsync(context);

        var organizationId = await GetOrganizationId(context, member);
        if (string.IsNullOrEmpty(organizationId))
        {
            return [];
        }

        if (context.User != null && IsGloballyLocked(context.User))
        {
            return [GetGlobalLockoutError(context.User)];
        }

        var availableOrganizationIds = GetAvailableOrganizationIds(member);
        if (!availableOrganizationIds.Contains(organizationId))
        {
            return await HandleUnavailableOrganizationAsync(context, member, organizationId, availableOrganizationIds);
        }

        if (context.User != null)
        {
            var accessError = await ValidateOrganizationAccessAsync(context, member, allowFallback, organizationId, availableOrganizationIds);
            if (accessError != null)
            {
                return [accessError];
            }
        }

        return [];
    }

    private async Task<IList<TokenResponse>> HandleUnavailableOrganizationAsync(
        TokenRequestContext context, Member member, string organizationId, IList<string> availableOrganizationIds)
    {
        if (context.Request.GrantType != OpenIddictConstants.GrantTypes.Password)
        {
            return [ErrorDescriber.InvalidOrganizationId(organizationId)];
        }

        var accessibleOrganizationIds = await OrganizationAccessResolver.GetAccessibleOrganizationIdsAsync(
            organizationMembershipSearchService, context.User?.Id, member, availableOrganizationIds);
        context.Request.SetParameter(Parameters.OrganizationId, accessibleOrganizationIds.FirstOrDefault());

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

    private async Task<TokenResponse> ValidateOrganizationAccessAsync(
        TokenRequestContext context, Member member, bool allowFallback, string organizationId, IList<string> availableOrganizationIds)
    {
        var membership = await organizationMembershipSearchService.GetMembershipAsync(context.User.Id, organizationId);

        var isLocked = membership != null && membership.IsCurrentlyLocked;
        var effectiveStatus = OrganizationMembership.ResolveEffectiveStatus(membership?.Status, member?.Status);
        var statusError = GetStatusError(effectiveStatus, organizationId);

        if (!isLocked && statusError == null)
        {
            return null;
        }

        // Locked/blocked in this one organization must not lock the user out of the others they belong to.
        if (allowFallback)
        {
            var accessibleOrganizationIds = await OrganizationAccessResolver.GetAccessibleOrganizationIdsAsync(
                organizationMembershipSearchService, context.User.Id, member, availableOrganizationIds);
            var fallbackOrganizationId = accessibleOrganizationIds.FirstOrDefault();

            if (!string.IsNullOrEmpty(fallbackOrganizationId))
            {
                context.Request.SetParameter(Parameters.OrganizationId, fallbackOrganizationId);
                return null;
            }
        }

        return isLocked ? ErrorDescriber.UserIsLockedInOrganization(organizationId) : statusError;
    }

    private static TokenResponse GetStatusError(string effectiveStatus, string organizationId)
    {
        return effectiveStatus switch
        {
            ModuleConstants.MembershipStatuses.Invited => ErrorDescriber.UserInvitationPendingInOrganization(organizationId),
            ModuleConstants.MembershipStatuses.Rejected => ErrorDescriber.UserIsRejectedInOrganization(organizationId),
            ModuleConstants.MembershipStatuses.Deleted => ErrorDescriber.UserIsRemovedFromOrganization(organizationId),
            _ => null,
        };
    }

    private static bool IsGloballyLocked(ApplicationUser user)
    {
        return user.LockoutEnabled
            && user.LockoutEnd.HasValue
            && user.LockoutEnd.Value > DateTimeOffset.UtcNow;
    }

    private async Task<string> GetOrganizationId(TokenRequestContext context, Member member)
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

        return await OrganizationAccessResolver.ResolveOrganizationIdAsync(organizationMembershipSearchService, context.User?.Id, member);
    }

    private async Task<Member> GetMemberAsync(TokenRequestContext context)
    {
        var memberId = context.User?.MemberId;

        return string.IsNullOrEmpty(memberId)
            ? null
            : await memberService.GetByIdAsync(memberId);
    }

    private static IList<string> GetAvailableOrganizationIds(Member member)
    {
        return member switch
        {
            Contact contact => contact.Organizations ?? [],
            Employee employee => employee.Organizations ?? [],
            _ => [],
        };
    }
}

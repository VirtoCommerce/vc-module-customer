using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core;
using VirtoCommerce.CustomerModule.Core.Extensions;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Security.SignInLog;
using VirtoCommerce.Platform.Security.OpenIddict;
using static OpenIddict.Abstractions.OpenIddictConstants.GrantTypes;
using static VirtoCommerce.CustomerModule.Core.ModuleConstants.Security;

namespace VirtoCommerce.CustomerModule.Data.OpenIddict;

public class OrganizationIdRequestValidator(
    IMemberService memberService,
    IOrganizationMembershipSearchService organizationMembershipSearchService) : ITokenRequestValidator
{
    public virtual int Priority { get; set; } = 50;

    public virtual async Task<IList<TokenResponse>> ValidateAsync(TokenRequestContext context)
    {
        if (context.User is null || context.SignInResult is { Succeeded: false })
        {
            return [];
        }

        var member = await GetMemberAsync(context);
        var organizationId = await GetOrganizationId(context, member);

        if (string.IsNullOrEmpty(organizationId))
        {
            return [];
        }

        var availableOrganizationIds = GetAvailableOrganizationIds(member);

        // Only refresh_token with organization_id is a deliberate switch; other requests resend a remembered organization that may have become unavailable
        var allowFallback = context.Request.GrantType != RefreshToken ||
                            string.IsNullOrEmpty(GetRequestedOrganizationId(context));

        var error = availableOrganizationIds.Contains(organizationId)
            ? await ValidateOrganizationAccessAsync(context, member, allowFallback, organizationId, availableOrganizationIds)
            : await HandleUnavailableOrganizationAsync(context, member, allowFallback, organizationId, availableOrganizationIds);

        if (error is null)
        {
            return [];
        }

        context.FailureReason = SignInFailureReason.NotAllowed;

        return [error];
    }

    private async Task<TokenResponse> HandleUnavailableOrganizationAsync(
        TokenRequestContext context, Member member, bool allowFallback, string organizationId, IList<string> availableOrganizationIds)
    {
        if (!allowFallback)
        {
            return ErrorDescriber.InvalidOrganizationId(organizationId);
        }

        var accessibleOrganizationIds = await OrganizationAccessResolver.GetAccessibleOrganizationIdsAsync(
            organizationMembershipSearchService, context.User.Id, member, availableOrganizationIds);
        context.Request.SetParameter(Parameters.OrganizationId, accessibleOrganizationIds.FirstOrDefault());

        return null;
    }

    private async Task<TokenResponse> ValidateOrganizationAccessAsync(
        TokenRequestContext context,
        Member member,
        bool allowFallback,
        string organizationId,
        IList<string> availableOrganizationIds)
    {
        var membership = await organizationMembershipSearchService.GetMembershipAsync(context.User.Id, organizationId);

        var isLocked = membership != null && membership.IsCurrentlyLocked;
        var effectiveStatus = OrganizationMembership.ResolveEffectiveStatus(membership?.Status, member?.Status);
        var statusError = GetStatusError(effectiveStatus, organizationId);

        if (!isLocked && statusError is null)
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

            if (isLocked)
            {
                context.Request.SetParameter(Parameters.OrganizationId, null);
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

    private async Task<string> GetOrganizationId(TokenRequestContext context, Member member)
    {
        var organizationId = GetRequestedOrganizationId(context);
        if (!string.IsNullOrEmpty(organizationId))
        {
            return organizationId;
        }

        organizationId = context.Principal?.FindFirstValue(Claims.OrganizationId);
        if (!string.IsNullOrEmpty(organizationId))
        {
            return organizationId;
        }

        return await OrganizationAccessResolver.ResolveOrganizationIdAsync(organizationMembershipSearchService, context.User.Id, member);
    }

    private static string GetRequestedOrganizationId(TokenRequestContext context)
    {
        return context.Request.GetParameter(Parameters.OrganizationId)?.ToString();
    }

    private async Task<Member> GetMemberAsync(TokenRequestContext context)
    {
        var memberId = context.User.MemberId;

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

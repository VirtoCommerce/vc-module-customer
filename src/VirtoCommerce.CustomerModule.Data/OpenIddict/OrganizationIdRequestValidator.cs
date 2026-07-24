using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using OpenIddict.Abstractions;
using VirtoCommerce.CustomerModule.Core;
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
        var member = await GetMemberAsync(context);

        var organizationId = await GetOrganizationId(context, member);
        if (string.IsNullOrEmpty(organizationId))
        {
            return [];
        }

        if (context.User != null && IsGloballyLocked(context.User))
        {
            var permanentLockOut = context.User.LockoutEnd == DateTime.MaxValue.ToUniversalTime();
            return [permanentLockOut
                ? ErrorDescriber.UserIsLockedOut()
                : SecurityErrorDescriber.UserIsTemporaryLockedOut()];
        }

        var isPasswordGrant = context.Request.GrantType == OpenIddictConstants.GrantTypes.Password;

        var availableOrganizationIds = GetAvailableOrganizationIds(member);
        if (!availableOrganizationIds.Contains(organizationId))
        {
            if (isPasswordGrant)
            {
                var accessibleOrganizationIds = await GetAccessibleOrganizationIdsAsync(context.User?.Id, member, availableOrganizationIds);
                context.Request.SetParameter(Parameters.OrganizationId, accessibleOrganizationIds.FirstOrDefault());

                return [];
            }

            return [ErrorDescriber.InvalidOrganizationId(organizationId)];
        }

        if (context.User != null)
        {
            var membership = (await organizationMembershipSearchService.SearchAsync(new OrganizationMembershipSearchCriteria
            {
                UserId = context.User.Id,
                OrganizationId = organizationId,
                Take = 1,
            })).Results.FirstOrDefault();

            var isLocked = membership != null && membership.IsCurrentlyLocked;
            var effectiveStatus = OrganizationMembership.ResolveEffectiveStatus(membership?.Status, member?.Status);
            var statusError = GetStatusError(effectiveStatus, organizationId);

            if (isLocked || statusError != null)
            {
                if (isPasswordGrant)
                {
                    var accessibleOrganizationIds = await GetAccessibleOrganizationIdsAsync(context.User.Id, member, availableOrganizationIds);
                    var fallbackOrganizationId = accessibleOrganizationIds.FirstOrDefault();

                    if (!string.IsNullOrEmpty(fallbackOrganizationId))
                    {
                        context.Request.SetParameter(Parameters.OrganizationId, fallbackOrganizationId);
                        return [];
                    }
                }

                return [isLocked ? ErrorDescriber.UserIsLockedInOrganization(organizationId) : statusError];
            }
        }

        return [];
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

        return await GetMemberOrganizationId(context, member);
    }

    private async Task<string> GetMemberOrganizationId(TokenRequestContext context, Member member)
    {
        if (member is not IHasOrganizations contact)
        {
            return null;
        }

        var organizations = contact.Organizations ?? [];

        var accessibleOrganizations = await GetAccessibleOrganizationIdsAsync(context.User?.Id, member, organizations);

        if (!contact.CurrentOrganizationId.IsNullOrEmpty() && accessibleOrganizations.Contains(contact.CurrentOrganizationId, StringComparer.OrdinalIgnoreCase))
        {
            return contact.CurrentOrganizationId;
        }

        if (!contact.DefaultOrganizationId.IsNullOrEmpty() && accessibleOrganizations.Contains(contact.DefaultOrganizationId, StringComparer.OrdinalIgnoreCase))
        {
            return contact.DefaultOrganizationId;
        }

        return accessibleOrganizations.FirstOrDefault();
    }

    private async Task<IReadOnlyCollection<string>> GetAccessibleOrganizationIdsAsync(string userId, Member member, IList<string> organizationIds)
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

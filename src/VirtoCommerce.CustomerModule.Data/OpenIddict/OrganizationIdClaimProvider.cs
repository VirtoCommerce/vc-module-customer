using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using OpenIddict.Abstractions;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Security.Extensions;
using VirtoCommerce.Platform.Security.OpenIddict;
using static VirtoCommerce.CustomerModule.Core.ModuleConstants.Security;

namespace VirtoCommerce.CustomerModule.Data.OpenIddict;

public class OrganizationIdClaimProvider(IMemberService memberService) : ITokenClaimProvider
{
    public virtual async Task SetClaimsAsync(ClaimsPrincipal principal, TokenRequestContext context)
    {
        var organizationId = await GetOrganizationId(context);
        principal.SetClaimWithDestinations(Claims.OrganizationId, organizationId, [OpenIddictConstants.Destinations.AccessToken]);
    }


    private async Task<string> GetOrganizationId(TokenRequestContext context)
    {
        var organizationId = context.Request.GetParameter(Parameters.OrganizationId)?.Value?.ToString();
        if (!string.IsNullOrEmpty(organizationId))
        {
            return organizationId;
        }

        organizationId = context.Principal?.FindFirstValue(Claims.OrganizationId);
        if (!string.IsNullOrEmpty(organizationId))
        {
            return organizationId;
        }

        return await GetDefaultOrganizationId(context);
    }

    private async Task<string> GetDefaultOrganizationId(TokenRequestContext context)
    {
        var memberId = context.User?.MemberId;
        if (string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        var member = await memberService.GetByIdAsync(memberId);

        return member switch
        {
            Contact contact => contact.DefaultOrganizationId ?? contact.Organizations?.FirstOrDefault(),
            Employee employee => employee.DefaultOrganizationId ?? employee.Organizations?.FirstOrDefault(),
            _ => null,
        };
    }
}

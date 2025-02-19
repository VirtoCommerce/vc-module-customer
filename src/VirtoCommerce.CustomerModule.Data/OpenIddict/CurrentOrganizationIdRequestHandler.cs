using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.Extensions;
using VirtoCommerce.Platform.Security.OpenIddict;

namespace VirtoCommerce.CustomerModule.Data.OpenIddict;

public class CurrentOrganizationIdRequestHandler(IMemberService memberService) : ITokenRequestHandler
{
    public async Task HandleAsync(ApplicationUser user, TokenRequestContext context)
    {
        // skip currentOrganizationId change for any form of impersonation
        if (context.Request.GrantType == PlatformConstants.Security.GrantTypes.Impersonate ||
            (context.Principal != null && context.Principal.IsImpersonated()))
        {
            return;
        }

        var organizationId = context.Request.GetParameter(Parameters.OrganizationId)?.Value?.ToString();
        if (string.IsNullOrEmpty(organizationId))
        {
            return;
        }

        var memberId = context.User?.MemberId;
        if (string.IsNullOrEmpty(memberId))
        {
            return;
        }

        if (await memberService.GetByIdAsync(memberId) is not IHasOrganizations member)
        {
            return;
        }

        if (member.CurrentOrganizationId != organizationId)
        {
            member.CurrentOrganizationId = organizationId;
            await memberService.SaveChangesAsync([(Member)member]);
        }
    }
}

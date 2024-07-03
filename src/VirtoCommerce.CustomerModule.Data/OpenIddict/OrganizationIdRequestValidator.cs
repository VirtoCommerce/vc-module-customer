using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using OpenIddict.Abstractions;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Security.OpenIddict;
using static VirtoCommerce.CustomerModule.Core.ModuleConstants.Security;

namespace VirtoCommerce.CustomerModule.Data.OpenIddict;

public class OrganizationIdRequestValidator(IMemberService memberService) : ITokenRequestValidator
{
    public virtual int Priority { get; set; } = 50;

    private readonly IList<TokenResponse> _valid = [];

    public virtual async Task<IList<TokenResponse>> ValidateAsync(TokenRequestContext context)
    {
        var organizationId = GetOrganizationId(context);
        if (string.IsNullOrEmpty(organizationId))
        {
            return _valid;
        }

        var availableOrganizationIds = await GetAvailableOrganizationIds(context);
        if (availableOrganizationIds.Contains(organizationId))
        {
            return _valid;
        }

        // Replace invalid organization ID with default organization ID when signing in
        if (context.Request.GrantType == OpenIddictConstants.GrantTypes.Password)
        {
            context.Request.SetParameter(Parameters.OrganizationId, availableOrganizationIds.FirstOrDefault());
            return _valid;
        }

        return [ErrorDescriber.InvalidOrganizationId(organizationId)];
    }

    private static string GetOrganizationId(TokenRequestContext context)
    {
        var organizationId = context.Request.GetParameter(Parameters.OrganizationId)?.Value?.ToString();
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

        var member = await memberService.GetByIdAsync(memberId);

        return member switch
        {
            Contact contact => contact.Organizations ?? [],
            Employee employee => employee.Organizations ?? [],
            _ => [],
        };
    }
}

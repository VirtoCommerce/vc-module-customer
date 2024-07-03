using System.Security.Claims;
using static VirtoCommerce.CustomerModule.Core.ModuleConstants.Security;

namespace VirtoCommerce.CustomerModule.Core.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetCurrentOrganizationId(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal?.FindFirstValue(Claims.OrganizationId);
    }
}

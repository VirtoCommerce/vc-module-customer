using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;

namespace VirtoCommerce.CustomerModule.Core.Extensions;

public static class OrganizationMembershipSearchServiceExtensions
{
    public static async Task<OrganizationMembership> GetMembershipAsync(
        this IOrganizationMembershipSearchService service, string userId, string organizationId)
    {
        var result = await service.SearchAsync(new OrganizationMembershipSearchCriteria
        {
            UserId = userId,
            OrganizationId = organizationId,
            Take = 1,
        });

        return result.Results.FirstOrDefault();
    }
}

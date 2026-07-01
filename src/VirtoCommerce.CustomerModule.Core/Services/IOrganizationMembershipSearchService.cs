using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CustomerModule.Core.Services;

public interface IOrganizationMembershipSearchService
    : ISearchService<OrganizationMembershipSearchCriteria, OrganizationMembershipSearchResult, OrganizationMembership>
{
    /// <summary>
    /// Returns, for every user that has at least one membership matching the criteria,
    /// the number of matching memberships (i.e. organizations the user belongs to within the criteria).
    /// The aggregation runs in the database (one row per matching user), so it is safe for large datasets
    /// and avoids loading the full membership entities. Useful for finding all users granted a role
    /// (e.g. a permission's roles) across organizations and how many organizations each one serves.
    /// </summary>
    Task<IDictionary<string, int>> GetCountsByUserAsync(OrganizationMembershipSearchCriteria criteria);

    Task<IReadOnlyCollection<OrganizationRole>> GetRolesByUserAndOrgAsync(string userId, string organizationId);

    Task<IDictionary<string, IReadOnlyCollection<OrganizationRole>>> GetRolesForUsersInOrgAsync(IList<string> userIds, string organizationId);

    Task<IReadOnlyCollection<string>> GetUserIdsByRoleInOrgAsync(string organizationId, IList<string> roleIds);
}

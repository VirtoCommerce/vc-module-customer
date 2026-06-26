using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CustomerModule.Core.Services;

public interface IOrganizationMembershipService
    : ICrudService<OrganizationMembership>,
      ISearchService<OrganizationMembershipSearchCriteria,
      OrganizationMembershipSearchResult,
      OrganizationMembership>
{
    Task<OrganizationMembership> GetByUserAndOrgAsync(string userId, string organizationId);

    Task<IReadOnlyCollection<string>> GetLockedOrganizationIdsAsync(string userId);

    Task<int> CountByUserIdAsync(string userId);

    /// <summary>
    /// Returns the number of organizations each user is a member of, counting only memberships that
    /// carry at least one of the given <paramref name="roleIds"/>. Optionally scoped to specific
    /// organizations and/or users. The aggregation runs in the database (one row per matching user),
    /// so it is safe for large datasets. Useful for finding all users granted a role (e.g. a permission's
    /// roles) across organizations and how many organizations each one serves.
    /// </summary>
    Task<IDictionary<string, int>> GetOrganizationCountsByUserAsync(string[] roleIds, string[] organizationIds = null, string[] userIds = null);

    Task<OrganizationMembership> LockAsync(string id, DateTime? lockoutEnd = null);

    Task<OrganizationMembership> UnlockAsync(string id);
}

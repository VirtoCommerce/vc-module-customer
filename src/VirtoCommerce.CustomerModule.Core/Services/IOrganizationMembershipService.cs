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

    Task<IReadOnlyCollection<OrganizationRole>> GetRolesByUserAndOrgAsync(string userId, string organizationId);

    Task<IReadOnlyCollection<string>> GetUserIdsByRoleInOrgAsync(string organizationId, IList<string> roleIds);

    Task<IReadOnlyCollection<string>> GetLockedOrganizationIdsAsync(string userId);

    Task<int> CountByUserIdAsync(string userId);

    Task<OrganizationMembership> LockAsync(string id, DateTime? lockoutEnd = null);

    Task<OrganizationMembership> UnlockAsync(string id);
}

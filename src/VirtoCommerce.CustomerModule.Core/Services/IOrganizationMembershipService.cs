using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model;

namespace VirtoCommerce.CustomerModule.Core.Services;

public interface IOrganizationMembershipService
{
    Task<OrganizationMembership> GetByIdAsync(string id);

    Task<OrganizationMembership> GetByUserAndOrgAsync(string userId, string organizationId);

    /// <summary>Returns only the IDs of organizations where the user is currently locked. Lightweight — no roles loaded.</summary>
    Task<IReadOnlyCollection<string>> GetLockedOrganizationIdsAsync(string userId);

    Task<int> CountByUserIdAsync(string userId);

    Task<OrganizationMembershipSearchResult> SearchAsync(OrganizationMembershipSearchCriteria criteria);

    Task SaveChangesAsync(IList<OrganizationMembership> memberships);

    Task LockAsync(string id, DateTime? lockoutEnd = null);

    Task UnlockAsync(string id);

    Task DeleteAsync(IList<string> ids);
}

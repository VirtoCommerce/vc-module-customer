using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model;

namespace VirtoCommerce.CustomerModule.Core.Services;

public interface IOrganizationMembershipService
{
    Task<OrganizationMembership> GetByIdAsync(string id);

    Task<OrganizationMembership> GetByUserAndOrgAsync(string userId, string organizationId);

    Task<IList<OrganizationMembership>> GetByUserIdAsync(string userId);

    Task SaveChangesAsync(IList<OrganizationMembership> memberships);

    Task LockAsync(string id, DateTime? lockoutEnd = null);

    Task UnlockAsync(string id);

    Task DeleteAsync(IList<string> ids);
}

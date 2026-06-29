using System;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CustomerModule.Core.Services;

public interface IOrganizationMembershipService : ICrudService<OrganizationMembership>
{
    Task<OrganizationMembership> LockAsync(string id, DateTime? lockoutEnd = null);

    Task<OrganizationMembership> UnlockAsync(string id);
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace VirtoCommerce.CustomerModule.Core.Services;

public interface ICompanyMemberRoleService
{
    Task<IList<string>> GetAllowedRoleIdsAsync(string storeId);
}

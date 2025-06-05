using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Data.Model;

namespace VirtoCommerce.CustomerModule.Data.Repositories
{
    public interface ICustomerRepository : IMemberRepository
    {
        IQueryable<OrganizationEntity> Organizations { get; }
        IQueryable<ContactEntity> Contacts { get; }
        IQueryable<VendorEntity> Vendors { get; }
        IQueryable<EmployeeEntity> Employees { get; }
        IQueryable<CustomerPreferenceEntity> CustomerPreferences { get; }

        Task<IList<CustomerPreferenceEntity>> GetCustomerPreferencesByIdsAsync(IList<string> ids, string responseGroup);
    }
}

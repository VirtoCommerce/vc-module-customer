using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Data.Repositories
{
    public class CustomerRepository : MemberRepositoryBase, ICustomerRepository
    {
        public CustomerRepository(CustomerDbContext dbContext) : base(dbContext)
        {
        }

        public IQueryable<OrganizationEntity> Organizations => DbContext.Set<OrganizationEntity>();
        public IQueryable<ContactEntity> Contacts => DbContext.Set<ContactEntity>();
        public IQueryable<EmployeeEntity> Employees => DbContext.Set<EmployeeEntity>();
        public IQueryable<VendorEntity> Vendors => DbContext.Set<VendorEntity>();
        public IQueryable<CustomerPreferenceEntity> CustomerPreferences => DbContext.Set<CustomerPreferenceEntity>();
        public IQueryable<OrganizationMembershipEntity> OrganizationMemberships => DbContext.Set<OrganizationMembershipEntity>();
        public IQueryable<OrganizationMembershipRoleEntity> OrganizationMembershipRoles => DbContext.Set<OrganizationMembershipRoleEntity>();
        public IQueryable<OrganizationRoleEntity> OrganizationRoles => DbContext.Set<OrganizationRoleEntity>();

        public override async Task<T[]> InnerGetMembersByIds<T>(string[] ids, string responseGroup)
        {
            var result = await base.InnerGetMembersByIds<T>(ids, responseGroup);

            var memberResponseGroup = EnumUtility.SafeParseFlags(responseGroup, MemberResponseGroup.Full);
            if (memberResponseGroup.HasFlag(MemberResponseGroup.WithRoles))
            {
                var orgIds = result.OfType<OrganizationEntity>().Select(x => x.Id).ToArray();
                if (orgIds.Length > 0)
                {
                    await OrganizationRoles.Where(x => orgIds.Contains(x.OrganizationId)).LoadAsync();
                }
            }

            return result;
        }

        public virtual async Task<IList<CustomerPreferenceEntity>> GetCustomerPreferencesByIdsAsync(IList<string> ids, string responseGroup)
        {
            if (ids.IsNullOrEmpty())
            {
                return [];
            }

            return ids.Count == 1
                ? await CustomerPreferences.Where(x => x.Id == ids.First()).ToListAsync()
                : await CustomerPreferences.Where(x => ids.Contains(x.Id)).ToListAsync();
        }
    }
}

using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

namespace VirtoCommerce.CustomerModule.Data.Repositories
{
    public class CustomerRepositoryImpl : MemberRepositoryBase, ICustomerRepository
    {
        public CustomerRepositoryImpl()
        {
        }

        public CustomerRepositoryImpl(string nameOrConnectionString, params IInterceptor[] interceptors)
            : base(nameOrConnectionString, interceptors)
        {
            Configuration.ProxyCreationEnabled = false;
        }

        public CustomerRepositoryImpl(DbConnection existingConnection, IUnitOfWork unitOfWork = null,
            IInterceptor[] interceptors = null) : base(existingConnection, unitOfWork, interceptors)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            #region Contact
            modelBuilder.Entity<ContactDataEntity>().HasKey(x => x.Id)
                .Property(x => x.Id);
            modelBuilder.Entity<ContactDataEntity>().ToTable("Contact");

            #endregion

            #region Organization
            modelBuilder.Entity<OrganizationDataEntity>().HasKey(x => x.Id)
                .Property(x => x.Id);
            modelBuilder.Entity<OrganizationDataEntity>().ToTable("Organization");
            #endregion

            #region Employee
            modelBuilder.Entity<EmployeeDataEntity>().HasKey(x => x.Id)
                .Property(x => x.Id);
            modelBuilder.Entity<EmployeeDataEntity>().ToTable("Employee");

            #endregion

            #region Vendor
            modelBuilder.Entity<VendorDataEntity>().HasKey(x => x.Id)
                .Property(x => x.Id);
            modelBuilder.Entity<VendorDataEntity>().ToTable("Vendor");

            #endregion

            base.OnModelCreating(modelBuilder);
        }


        #region ICustomerRepository Members
        public IQueryable<OrganizationDataEntity> Organizations
        {
            get { return GetAsQueryable<OrganizationDataEntity>(); }
        }

        public IQueryable<ContactDataEntity> Contacts
        {
            get { return GetAsQueryable<ContactDataEntity>(); }
        }

        public IQueryable<EmployeeDataEntity> Employees
        {
            get { return GetAsQueryable<EmployeeDataEntity>(); }
        }

        public IQueryable<VendorDataEntity> Vendors
        {
            get { return GetAsQueryable<VendorDataEntity>(); }
        }
        #endregion
    }

}

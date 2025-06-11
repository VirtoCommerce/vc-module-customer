using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.CustomerSampleModule.Web.Model;

namespace VirtoCommerce.CustomerSampleModule.Web.Repositories
{
    public class CustomerSampleDbContext : CustomerDbContext
    {
        public CustomerSampleDbContext(DbContextOptions<CustomerSampleDbContext> options)
            : base(options)
        {
        }

        protected CustomerSampleDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Contact2Entity>();
            modelBuilder.Entity<SupplierEntity>();
        }
    }
}

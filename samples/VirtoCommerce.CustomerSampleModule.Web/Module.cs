using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Model.Search;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.CustomerSampleModule.Web.Model;
using VirtoCommerce.CustomerSampleModule.Web.Repositories;
using VirtoCommerce.CustomerSampleModule.Web.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CustomerSampleModule.Web
{
    public class Module : IModule
    {
        public ManifestModuleInfo ModuleInfo { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<CustomerSampleDbContext>((provider, options) =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                options.UseSqlServer(configuration.GetConnectionString(ModuleInfo.Id) ?? configuration.GetConnectionString("VirtoCommerce.Customer") ?? configuration.GetConnectionString("VirtoCommerce"));
            });

            serviceCollection.AddTransient<ICustomerRepository, CustomerSampleRepositoryImpl>();
            serviceCollection.AddTransient<IMemberService, MemberService2>();
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            AbstractTypeFactory<MemberEntity>.RegisterType<SupplierEntity>();
            AbstractTypeFactory<Member>.RegisterType<Supplier>().MapToType<SupplierEntity>();

            // working with dynamic properties: https://virtocommerce.com/docs/latest/fundamentals/extensibility/using-dynamic-properties/
            var dynamicPropertyRegistrar = appBuilder.ApplicationServices.GetRequiredService<IDynamicPropertyRegistrar>();
            dynamicPropertyRegistrar.RegisterType<Supplier>();

            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.AllSettings, ModuleInfo.Id);

            AbstractTypeFactory<Contact>.OverrideType<Contact, Contact2>().MapToType<Contact2Entity>();
            AbstractTypeFactory<Member>.OverrideType<Contact, Contact2>().MapToType<Contact2Entity>();
            AbstractTypeFactory<MemberEntity>.OverrideType<ContactEntity, Contact2Entity>();
            AbstractTypeFactory<MembersSearchCriteria>.RegisterType<Contact2SearchCriteria>();

            using var serviceScope = appBuilder.ApplicationServices.CreateScope();
            using var dbContext = serviceScope.ServiceProvider.GetRequiredService<CustomerSampleDbContext>();
            dbContext.Database.EnsureCreated();
            dbContext.Database.Migrate();
        }

        public void Uninstall()
        {
            // Method intentionally left empty.
        }
    }
}

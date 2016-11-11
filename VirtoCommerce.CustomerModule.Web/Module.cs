using System;
using System.Web.Http;
using Microsoft.Practices.Unity;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.CustomerModule.Data.Services;
using VirtoCommerce.CustomerModule.Web.ExportImport;
using VirtoCommerce.CustomerModule.Web.JsonConverters;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Domain.Customer.Events;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

namespace VirtoCommerce.CustomerModule.Web
{
    public class Module : ModuleBase, ISupportExportImportModule
    {
        private const string _connectionStringName = "VirtoCommerce";
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule Members

        public override void SetupDatabase()
        {
            using (var db = new CustomerRepositoryImpl(_connectionStringName, _container.Resolve<AuditableInterceptor>()))
            {
                var initializer = new SetupDatabaseInitializer<CustomerRepositoryImpl, Data.Migrations.Configuration>();
                initializer.InitializeDatabase(db);
            }

        }

        public override void Initialize()
        {
            //Member changing event publisher.
            _container.RegisterType<IEventPublisher<MemberChangingEvent>, EventPublisher<MemberChangingEvent>>();

            Func<CustomerRepositoryImpl> customerRepositoryFactory = () => new CustomerRepositoryImpl(_connectionStringName, new EntityPrimaryKeyGeneratorInterceptor(), _container.Resolve<AuditableInterceptor>());
            _container.RegisterType<ICustomerRepository>(new InjectionFactory(c => customerRepositoryFactory));
            _container.RegisterType<IMemberRepository>(new InjectionFactory(c => customerRepositoryFactory));

            _container.RegisterType<IMemberService, CommerceMembersServiceImpl>();
            _container.RegisterType<IMemberSearchService, CommerceMembersServiceImpl>();
        }

        public override void PostInitialize()
        {          
            AbstractTypeFactory<Member>.RegisterType<Organization>().MapToType<OrganizationDataEntity>();
            AbstractTypeFactory<Member>.RegisterType<Contact>().MapToType<ContactDataEntity>();
            AbstractTypeFactory<Member>.RegisterType<Vendor>().MapToType<VendorDataEntity>();
            AbstractTypeFactory<Member>.RegisterType<Employee>().MapToType<EmployeeDataEntity>();

            AbstractTypeFactory<MemberDataEntity>.RegisterType<ContactDataEntity>();
            AbstractTypeFactory<MemberDataEntity>.RegisterType<OrganizationDataEntity>();
            AbstractTypeFactory<MemberDataEntity>.RegisterType<VendorDataEntity>();
            AbstractTypeFactory<MemberDataEntity>.RegisterType<EmployeeDataEntity>();

            //Next lines allow to use polymorph types in API controller methods
            var httpConfiguration = _container.Resolve<HttpConfiguration>();
            httpConfiguration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new PolymorphicMemberJsonConverter());

            base.PostInitialize();
        }

        #endregion

        #region ISupportExportImportModule Members

        public void DoExport(System.IO.Stream outStream, PlatformExportManifest manifest, Action<ExportImportProgressInfo> progressCallback)
        {
            var exportJob = _container.Resolve<CustomerExportImport>();
            exportJob.DoExport(outStream, progressCallback);
        }

        public void DoImport(System.IO.Stream inputStream, PlatformExportManifest manifest, Action<ExportImportProgressInfo> progressCallback)
        {
            var exportJob = _container.Resolve<CustomerExportImport>();
            exportJob.DoImport(inputStream, progressCallback);
        }

        public string ExportDescription
        {
            get
            {
                var settingManager = _container.Resolve<ISettingsManager>();
                return settingManager.GetValue("Customer.ExportImport.Description", String.Empty);
            }
        }
        #endregion
    }
}

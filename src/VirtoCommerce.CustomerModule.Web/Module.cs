using System;
using System.IO;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CustomerModule.Core;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Notifications;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Core.Services.Indexed;
using VirtoCommerce.CustomerModule.Data.ExportImport;
using VirtoCommerce.CustomerModule.Data.Handlers;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.CustomerModule.Data.MySql;
using VirtoCommerce.CustomerModule.Data.PostgreSql;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.CustomerModule.Data.Search;
using VirtoCommerce.CustomerModule.Data.Search.Indexing;
using VirtoCommerce.CustomerModule.Data.Services;
using VirtoCommerce.CustomerModule.Data.SqlServer;
using VirtoCommerce.CustomerModule.Data.Validation;
using VirtoCommerce.CustomerModule.Web.Authorization;
using VirtoCommerce.NotificationsModule.Core.Services;
using VirtoCommerce.Platform.Core.Bus;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.JsonConverters;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Security.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.CustomerModule.Web
{
    public class Module : IModule, IExportSupport, IImportSupport, IHasConfiguration
    {
        private IApplicationBuilder _appBuilder;

        public ManifestModuleInfo ModuleInfo { get; set; }
        public IConfiguration Configuration { get; set; }


        public void Initialize(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<CustomerDbContext>(options =>
            {
                var databaseProvider = Configuration.GetValue("DatabaseProvider", "SqlServer");
                var connectionString = Configuration.GetConnectionString(ModuleInfo.Id) ?? Configuration.GetConnectionString("VirtoCommerce");

                switch (databaseProvider)
                {
                    case "MySql":
                        options.UseMySqlDatabase(connectionString);
                        break;
                    case "PostgreSql":
                        options.UsePostgreSqlDatabase(connectionString);
                        break;
                    default:
                        options.UseSqlServerDatabase(connectionString);
                        break;
                }
            });


            serviceCollection.AddTransient<ICustomerRepository, CustomerRepository>();
            serviceCollection.AddSingleton<Func<ICustomerRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetRequiredService<ICustomerRepository>());
            serviceCollection.AddSingleton<Func<IMemberRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetRequiredService<ICustomerRepository>());

            serviceCollection.AddTransient<IIndexedMemberSearchService, MemberIndexedSearchService>();
            serviceCollection.AddTransient<IMemberSearchService, MemberSearchService>();
            serviceCollection.AddTransient<IMemberService, MemberService>();
            serviceCollection.AddTransient<IMemberResolver, MemberResolver>();
            serviceCollection.AddSingleton<CustomerExportImport>();
            serviceCollection.AddTransient<MemberSearchRequestBuilder>();
            serviceCollection.AddSingleton<IFavoriteAddressService, FavoriteAddressService>();

            serviceCollection.AddSingleton<MemberDocumentChangesProvider>();
            serviceCollection.AddSingleton<MemberDocumentBuilder>();

            serviceCollection.AddSingleton(provider => new IndexDocumentConfiguration
            {
                DocumentType = KnownDocumentTypes.Member,
                DocumentSource = new IndexDocumentSource
                {
                    ChangesProvider = provider.GetService<MemberDocumentChangesProvider>(),
                    DocumentBuilder = provider.GetService<MemberDocumentBuilder>(),
                },
            });

            serviceCollection.AddTransient<LogChangesEventHandler>();
            serviceCollection.AddTransient<SecurtityAccountChangesEventHandler>();
            serviceCollection.AddTransient<IndexMemberChangedEventHandler>();

            serviceCollection.AddTransient<IAuthorizationHandler, CustomerAuthorizationHandler>();

            serviceCollection.AddTransient<AbstractValidator<Member>, MemberValidator>();
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            _appBuilder = appBuilder;
            AbstractTypeFactory<Member>.RegisterType<Organization>().MapToType<OrganizationEntity>();
            AbstractTypeFactory<Member>.RegisterType<Contact>().MapToType<ContactEntity>();
            AbstractTypeFactory<Member>.RegisterType<Vendor>().MapToType<VendorEntity>();
            AbstractTypeFactory<Member>.RegisterType<Employee>().MapToType<EmployeeEntity>();

            AbstractTypeFactory<MemberEntity>.RegisterType<ContactEntity>();
            AbstractTypeFactory<MemberEntity>.RegisterType<OrganizationEntity>();
            AbstractTypeFactory<MemberEntity>.RegisterType<VendorEntity>();
            AbstractTypeFactory<MemberEntity>.RegisterType<EmployeeEntity>();

            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.AllSettings, ModuleInfo.Id);
            settingsRegistrar.RegisterSettingsForType(ModuleConstants.Settings.StoreLevelSettings, nameof(Store));

            var dynamicPropertyRegistrar = appBuilder.ApplicationServices.GetRequiredService<IDynamicPropertyRegistrar>();
            dynamicPropertyRegistrar.RegisterType<Organization>();
            dynamicPropertyRegistrar.RegisterType<Contact>();
            dynamicPropertyRegistrar.RegisterType<Vendor>();
            dynamicPropertyRegistrar.RegisterType<Employee>();

            var permissionsRegistrar = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
            permissionsRegistrar.RegisterPermissions(ModuleInfo.Id, "Customer", ModuleConstants.Security.Permissions.AllPermissions);

            AbstractTypeFactory<PermissionScope>.RegisterType<AssociatedOrganizationsOnlyScope>();

            permissionsRegistrar.WithAvailabeScopesForPermissions(new[] {
                ModuleConstants.Security.Permissions.Create,
                ModuleConstants.Security.Permissions.Access,
                ModuleConstants.Security.Permissions.Read,
                ModuleConstants.Security.Permissions.Update
            }, new AssociatedOrganizationsOnlyScope());

            PolymorphJsonConverter.RegisterTypeForDiscriminator(typeof(Member), nameof(Member.MemberType));

            var inProcessBus = appBuilder.ApplicationServices.GetService<IHandlerRegistrar>();
            inProcessBus.RegisterHandler<MemberChangedEvent>(async (message, _) => await appBuilder.ApplicationServices.GetService<LogChangesEventHandler>().Handle(message));
            inProcessBus.RegisterHandler<UserChangedEvent>(async (message, _) => await appBuilder.ApplicationServices.GetService<LogChangesEventHandler>().Handle(message));
            inProcessBus.RegisterHandler<UserRoleAddedEvent>(async (message, _) => await appBuilder.ApplicationServices.GetService<LogChangesEventHandler>().Handle(message));
            inProcessBus.RegisterHandler<UserRoleRemovedEvent>(async (message, _) => await appBuilder.ApplicationServices.GetService<LogChangesEventHandler>().Handle(message));
            inProcessBus.RegisterHandler<UserChangedEvent>(async (message, _) => await appBuilder.ApplicationServices.GetService<SecurtityAccountChangesEventHandler>().Handle(message));

            var settingsManager = appBuilder.ApplicationServices.GetService<ISettingsManager>();
            if (settingsManager.GetValue<bool>(ModuleConstants.Settings.General.EventBasedIndexation))
            {
                inProcessBus.RegisterHandler<MemberChangedEvent>(async (message, _) => await appBuilder.ApplicationServices.GetService<IndexMemberChangedEventHandler>().Handle(message));
                inProcessBus.RegisterHandler<UserChangedEvent>(async (message, _) => await appBuilder.ApplicationServices.GetService<IndexMemberChangedEventHandler>().Handle(message));
                inProcessBus.RegisterHandler<UserRoleAddedEvent>(async (message, _) => await appBuilder.ApplicationServices.GetService<IndexMemberChangedEventHandler>().Handle(message));
                inProcessBus.RegisterHandler<UserRoleRemovedEvent>(async (message, _) => await appBuilder.ApplicationServices.GetService<IndexMemberChangedEventHandler>().Handle(message));
            }

            var searchRequestBuilderRegistrar = appBuilder.ApplicationServices.GetService<ISearchRequestBuilderRegistrar>();

            searchRequestBuilderRegistrar.Register(KnownDocumentTypes.Member, appBuilder.ApplicationServices.GetService<MemberSearchRequestBuilder>);

            using (var serviceScope = appBuilder.ApplicationServices.CreateScope())
            {
                var databaseProvider = Configuration.GetValue("DatabaseProvider", "SqlServer");

                var dbContext = serviceScope.ServiceProvider.GetRequiredService<CustomerDbContext>();
                if (databaseProvider == "SqlServer")
                {
                    dbContext.Database.MigrateIfNotApplied(MigrationName.GetUpdateV2MigrationName(ModuleInfo.Id));
                }
                dbContext.Database.Migrate();
            }

            var notificationRegistrar = appBuilder.ApplicationServices.GetService<INotificationRegistrar>();
            notificationRegistrar.RegisterNotification<RegisterCompanyEmailNotification>();
        }

        public void Uninstall()
        {
            // Nothing to do here
        }

        public Task ExportAsync(Stream outStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback,
            ICancellationToken cancellationToken)
        {
            return _appBuilder.ApplicationServices.GetRequiredService<CustomerExportImport>().ExportAsync(outStream, options, progressCallback, cancellationToken);
        }

        public Task ImportAsync(Stream inputStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback,
            ICancellationToken cancellationToken)
        {
            return _appBuilder.ApplicationServices.GetRequiredService<CustomerExportImport>().ImportAsync(inputStream, options, progressCallback, cancellationToken);
        }
    }
}

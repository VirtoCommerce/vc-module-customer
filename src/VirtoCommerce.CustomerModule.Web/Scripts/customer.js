//Call this to register our module to main application
var moduleName = 'virtoCommerce.customerModule';

if (AppDependencies != undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .config(
        ['$stateProvider', function ($stateProvider) {
            $stateProvider
                .state('workspace.customerModule', {
                    url: '/customers',
                    templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                    controller: [
                        '$scope', '$location', 'platformWebApp.bladeNavigationService', 'virtoCommerce.customerModule.members', 'virtoCommerce.customerModule.memberTypesResolverService',
                        function ($scope, $location, bladeNavigationService, members, memberTypesResolverService) {
                            var memberId = $location.search().memberId;

                            if (memberId) {
                                members.get({ id: memberId }, function (listItem) {
                                    var foundTemplate = memberTypesResolverService.resolve(listItem.memberType);
                                    if (foundTemplate) {
                                        var newBlade = angular.copy(foundTemplate.detailBlade);
                                        newBlade.id = 'members';
                                        newBlade.level = 0;
                                        newBlade.isClosingDisabled = true;
                                        newBlade.currentEntity = listItem;
                                        newBlade.currentEntityId = listItem.id;
                                        newBlade.isNew = false;
                                        bladeNavigationService.showBlade(newBlade);
                                    } else {
                                        dialogService.showNotificationDialog({
                                            id: "error",
                                            title: "customer.dialogs.unknown-member-type.title",
                                            message: "customer.dialogs.unknown-member-type.message",
                                            messageValues: { memberType: listItem.memberType },
                                        });
                                    }
                                });
                            }
                            else {
                                var newBlade = {
                                    id: 'members',
                                    level: 0,
                                    currentEntity: { id: null },
                                    controller: 'virtoCommerce.customerModule.memberListController',
                                    template: 'Modules/$(VirtoCommerce.Customer)/Scripts/blades/member-list.tpl.html',
                                    isClosingDisabled: true
                                };
                                bladeNavigationService.showBlade(newBlade);

                            }
                        }
                    ]
                });
        }]
    )
    // define search filters to be accessible platform-wide
    .factory('virtoCommerce.customerModule.predefinedSearchFilters', ['$localStorage', function ($localStorage) {
        $localStorage.customerSearchFilters = $localStorage.customerSearchFilters || [];

        return {
            register: function (currentFiltersUpdateTime, currentFiltersStorageKey, newFilters) {
                _.each(newFilters, function (newFilter) {
                    var found = _.find($localStorage.customerSearchFilters, function (x) {
                        return x.id == newFilter.id;
                    });
                    if (found) {
                        if (!found.lastUpdateTime || found.lastUpdateTime < currentFiltersUpdateTime) {
                            angular.copy(newFilter, found);
                        }
                    } else if (!$localStorage[currentFiltersStorageKey] || $localStorage[currentFiltersStorageKey] < currentFiltersUpdateTime) {
                        $localStorage.customerSearchFilters.splice(0, 0, newFilter);
                    }
                });

                $localStorage[currentFiltersStorageKey] = currentFiltersUpdateTime;
            }
        };
    }])
    .run([
        '$rootScope', '$state', 'platformWebApp.mainMenuService', 'platformWebApp.authService', 'platformWebApp.permissionScopeResolver',
        'platformWebApp.widgetService', 'platformWebApp.settings', 'platformWebApp.userProfileIconService', 'platformWebApp.metaFormsService',
        'virtoCommerce.customerModule.memberTypesResolverService', 'virtoCommerce.customerModule.predefinedSearchFilters', 'virtoCommerce.customerModule.members',
        'platformWebApp.toolbarService', 'platformWebApp.bladeNavigationService',
        function (
            $rootScope, $state, mainMenuService, authService, scopeResolver,
            widgetService, settings, userProfileIconService, metaFormsService,
            memberTypesResolverService, predefinedSearchFilters, membersApi,
            toolbarService, bladeNavigationService) {
            //Register module in main menu
            var menuItem = {
                path: 'browse/member',
                icon: 'fas fa-address-card',
                title: 'customer.main-menu-title',
                priority: 180,
                action: function () { $state.go('workspace.customerModule', {}, { reload: true }); },
                permission: 'customer:access'
            };
            mainMenuService.addMenuItem(menuItem);

            // register back-button
            //toolbarService.register(breadcrumbHistoryService.getBackButtonInstance(), 'virtoCommerce.customerModule.memberListController');

            // create required WIDGETS
            var accountsWidget = {
                controller: 'virtoCommerce.customerModule.customerAccountsWidgetController',
                template: 'Modules/$(VirtoCommerce.Customer)/Scripts/widgets/customerAccountsWidget.tpl.html',
                isVisible: function (blade) { return !blade.isNew && authService.checkPermission('platform:security:read'); }
            };
            var addressesWidget = {
                controller: 'virtoCommerce.customerModule.memberAddressesWidgetController',
                template: 'Modules/$(VirtoCommerce.Customer)/Scripts/widgets/memberAddressesWidget.tpl.html'
            };
            var emailsWidget = {
                controller: 'virtoCommerce.customerModule.memberEmailsWidgetController',
                template: 'Modules/$(VirtoCommerce.Customer)/Scripts/widgets/memberEmailsWidget.tpl.html'
            };
            var phonesWidget = {
                controller: 'virtoCommerce.customerModule.memberPhonesWidgetController',
                template: 'Modules/$(VirtoCommerce.Customer)/Scripts/widgets/memberPhonesWidget.tpl.html'
            };
            var dynamicPropertyWidget = {
                controller: 'platformWebApp.dynamicPropertyWidgetController',
                template: '$(Platform)/Scripts/app/dynamicProperties/widgets/dynamicPropertyWidget.tpl.html',
                isVisible: function (blade) { return !blade.isNew && authService.checkPermission('platform:dynamic_properties:read'); }
            };
            var vendorSeoWidget = {
                controller: 'virtoCommerce.coreModule.seo.seoWidgetController',
                template: 'Modules/$(VirtoCommerce.Core)/Scripts/SEO/widgets/seoWidget.tpl.html',
                objectType: 'Vendor',
                getDefaultContainerId: function (blade) { return undefined; },
                getLanguages: function (blade) {
                    return settings.getValues({ id: 'VirtoCommerce.Core.General.Languages' });
                },
                isVisible: function (blade) { return !blade.isNew; }
            };
            var iconWidget = {
                controller: 'virtoCommerce.customerModule.memberIconWidgetController',
                template: 'Modules/$(VirtoCommerce.Customer)/Scripts/widgets/memberIconWidget.tpl.html',
                isVisible: function (blade) {
                    return authService.checkPermission('customer:create') ||
                        authService.checkPermission('customer:update');
                }
            }
            var indexWidget = {
                documentType: 'Member',
                controller: 'virtoCommerce.searchModule.indexWidgetController',
                template: 'Modules/$(VirtoCommerce.Search)/Scripts/widgets/index-widget.tpl.html',
                isVisible: function (blade) { return !blade.isNew; }
            };

            var assetsWidget = {
                controller: 'virtoCommerce.customerModule.organizationAssetsWidgetController',
                template: 'Modules/$(VirtoCommerce.Customer)/Scripts/widgets/organizationAssetsWidget.tpl.html',
                isVisible: function (blade) { return !blade.isNew; }
            };

            var accountContactWidget = {
                controller: 'virtoCommerce.customerModule.accountContactWidgetController',
                template: 'Modules/$(VirtoCommerce.Customer)/Scripts/widgets/accountContactWidget.tpl.html',
            };
            widgetService.registerWidget(accountContactWidget, 'accountDetail');

            //Register widgets in customer details
            widgetService.registerWidget(accountsWidget, 'customerDetail1');
            widgetService.registerWidget(addressesWidget, 'customerDetail1');
            widgetService.registerWidget(emailsWidget, 'customerDetail1');
            widgetService.registerWidget(phonesWidget, 'customerDetail2');
            widgetService.registerWidget(dynamicPropertyWidget, 'customerDetail2');
            widgetService.registerWidget(indexWidget, 'customerDetail2');
            widgetService.registerWidget(iconWidget, 'customerDetail2');

            //Register widgets in organization details
            widgetService.registerWidget(addressesWidget, 'organizationDetail1');
            widgetService.registerWidget(emailsWidget, 'organizationDetail1');
            widgetService.registerWidget(phonesWidget, 'organizationDetail1');
            widgetService.registerWidget(dynamicPropertyWidget, 'organizationDetail2');
            widgetService.registerWidget(indexWidget, 'organizationDetail2');
            widgetService.registerWidget(iconWidget, 'organizationDetail2');
            widgetService.registerWidget(assetsWidget, 'organizationDetail2');

            //Register widgets in employee details
            widgetService.registerWidget(accountsWidget, 'employeeDetail1');
            widgetService.registerWidget(addressesWidget, 'employeeDetail1');
            widgetService.registerWidget(emailsWidget, 'employeeDetail1');
            widgetService.registerWidget(phonesWidget, 'employeeDetail2');
            widgetService.registerWidget(dynamicPropertyWidget, 'employeeDetail2');
            widgetService.registerWidget(indexWidget, 'employeeDetail2');

            //Register widgets in vendor details
            widgetService.registerWidget(accountsWidget, 'vendorDetail1');
            widgetService.registerWidget(addressesWidget, 'vendorDetail1');
            widgetService.registerWidget(emailsWidget, 'vendorDetail1');
            widgetService.registerWidget(phonesWidget, 'vendorDetail1');
            widgetService.registerWidget(dynamicPropertyWidget, 'vendorDetail2');
            widgetService.registerWidget(vendorSeoWidget, 'vendorDetail2');
            widgetService.registerWidget(indexWidget, 'vendorDetail2');

            //Register widgets in user details
            widgetService.registerWidget(iconWidget, 'userProfileDetail');

            // register member types
            memberTypesResolverService.registerType({
                memberType: 'Organization',
                description: 'customer.blades.member-add.organization.description',
                fullTypeName: 'VirtoCommerce.CustomerModule.Core.Model.Organization',
                icon: 'fas fa-building',
                detailBlade: {
                    template: 'Modules/$(VirtoCommerce.Customer)/Scripts/blades/organization-detail.tpl.html',
                    metaFields0: [{
                        name: 'name',
                        title: "customer.blades.organization-detail.labels.name",
                        placeholder: "customer.blades.organization-detail.placeholders.name",
                        isRequired: true,
                        spanAllColumns: true,
                        valueType: "ShortText"
                    },
                    {
                        spanAllColumns: true,
                        templateUrl: "groups.html"
                    },
                    {
                        spanAllColumns: true,
                        templateUrl: "status.html"
                    }],
                    metaFields: [{
                        name: 'businessCategory',
                        title: "customer.blades.organization-detail.labels.business-category",
                        spanAllColumns: true,
                        valueType: "ShortText"
                    },
                    {
                        name: 'description',
                        title: "customer.blades.organization-detail.labels.description",
                        placeholder: "customer.blades.organization-detail.placeholders.description",
                        spanAllColumns: true,
                        valueType: "LongText"
                    }]
                },
                knownChildrenTypes: ['Organization', 'Employee', 'Contact']
            });
            memberTypesResolverService.registerType({
                memberType: 'Employee',
                description: 'customer.blades.member-add.employee.description',
                fullTypeName: 'VirtoCommerce.CustomerModule.Core.Model.Employee',
                icon: 'fas fa-user-tie',
                detailBlade: {
                    template: 'Modules/$(VirtoCommerce.Customer)/Scripts/blades/employee-detail.tpl.html',
                    metaFields0: [{
                        title: "customer.blades.employee-detail.labels.first-name",
                        templateUrl: "firstName.html"
                    }, {
                        name: 'middleName',
                        title: "customer.blades.employee-detail.labels.middle-name",
                        placeholder: "customer.blades.employee-detail.placeholders.middle-name",
                        valueType: "ShortText"
                    }, {
                        title: "customer.blades.employee-detail.labels.last-name",
                        templateUrl: "lastName.html"
                    }, {
                        title: "customer.blades.employee-detail.labels.full-name",
                        templateUrl: "fullName.html"
                    }, {
                        spanAllColumns: true,
                        templateUrl: "status.html"
                    }],
                    metaFields: [{
                        name: 'memberType',
                        title: "customer.blades.employee-detail.labels.type",
                        placeholder: "customer.blades.employee-detail.placeholders.type",
                        colSpan: 2,
                        disabled: true,
                        valueType: "ShortText"
                    },
                    {
                        colSpan: 2,
                        title: "customer.blades.employee-detail.labels.birthday",
                        placeholder: "customer.blades.employee-detail.placeholders.birthday",
                        templateUrl: "birthDate.html"
                    },
                    {
                        colSpan: 2,
                        title: "customer.blades.employee-detail.labels.organizations",
                        templateUrl: "organizations.html"
                    },
                    {
                        colSpan: 2,
                        title: "customer.blades.employee-detail.labels.default-organization",
                        templateUrl: "defaultOrganization.html"
                    },
                    {
                        colSpan: 2,
                        title: "customer.blades.employee-detail.labels.timezone",
                        templateUrl: "timeZone.html"
                    },
                    {
                        name: 'defaultLanguage',
                        title: "customer.blades.employee-detail.labels.defaultLanguage",
                        placeholder: "customer.blades.employee-detail.placeholders.defaultLanguage",
                        valueType: "ShortText"
                    },
                    {
                        name: 'photoUrl',
                        title: "customer.blades.employee-detail.labels.photo-url",
                        placeholder: "customer.blades.employee-detail.placeholders.photo-url",
                        valueType: "Url"
                    }]
                }
            });
            memberTypesResolverService.registerType({
                memberType: 'Contact',
                description: 'customer.blades.member-add.contact.description',
                fullTypeName: 'VirtoCommerce.CustomerModule.Core.Model.Contact',
                icon: 'far fa-smile',
                detailBlade: {
                    template: 'Modules/$(VirtoCommerce.Customer)/Scripts/blades/customer-detail.tpl.html',
                    metaFields0: [{
                        title: "customer.blades.contact-detail.labels.first-name",
                        templateUrl: "firstName.html"
                    },
                    {
                        title: "customer.blades.contact-detail.labels.last-name",
                        templateUrl: "lastName.html"
                    },
                    {
                        title: "customer.blades.contact-detail.labels.full-name",
                        templateUrl: "fullName.html",
                        colSpan: 2,
                    },
                    {
                        spanAllColumns: true,
                        templateUrl: "status.html"
                    },
                    {
                        templateUrl: "groups.html"
                    },
                    {
                        name: 'salutation',
                        title: "customer.blades.contact-detail.labels.salutation",
                        placeholder: "customer.blades.contact-detail.placeholders.salutation",
                        valueType: "ShortText"
                    }],
                    metaFields: [{
                        colSpan: 2,
                        title: "customer.blades.contact-detail.labels.organizations",
                        templateUrl: "organizations.html"
                    },
                    {
                        colSpan: 2,
                        title: "customer.blades.contact-detail.labels.default-organization",
                        templateUrl: "defaultOrganization.html"
                    },
                    {
                        colSpan: 2,
                        title: "customer.blades.contact-detail.labels.associated-organizations",
                        templateUrl: "associatedOrganizations.html"
                    },
                    {
                        colSpan: 2,
                        title: "customer.blades.contact-detail.labels.birthday",
                        templateUrl: "birthDate.html"
                    },
                    {
                        colSpan: 2,
                        title: "customer.blades.contact-detail.labels.timezone",
                        templateUrl: "timeZone.html"
                    },
                    {
                        title: "customer.blades.contact-detail.labels.defaultLanguage",
                        templateUrl: "language.html"
                    },
                    {
                        title: "customer.blades.contact-detail.labels.currency",
                        templateUrl: "currencyCode.html"
                    },
                    {
                        colSpan: 2,
                        name: 'taxPayerId',
                        title: "customer.blades.contact-detail.labels.taxpayerId",
                        placeholder: "customer.blades.contact-detail.placeholders.preferred-delivery",
                        valueType: "ShortText"
                    },
                    {
                        name: 'preferredCommunication',
                        title: "customer.blades.contact-detail.labels.preferred-communication",
                        placeholder: "customer.blades.contact-detail.placeholders.preferred-communication",
                        valueType: "ShortText"
                    },
                    {
                        name: 'preferredDelivery',
                        title: "customer.blades.contact-detail.labels.preferred-delivery",
                        placeholder: "customer.blades.contact-detail.placeholders.preferred-delivery",
                        valueType: "ShortText"
                    },
                    {
                        name: 'about',
                        title: "customer.blades.contact-detail.labels.about",
                        placeholder: "customer.blades.contact-detail.placeholders.about",
                        colSpan: 2,
                        valueType: "LongText"
                    }]
                }
            });
            memberTypesResolverService.registerType({
                memberType: 'Vendor',
                description: 'customer.blades.member-add.vendor.description',
                fullTypeName: 'VirtoCommerce.CustomerModule.Core.Model.Vendor',
                icon: 'fas fa-briefcase',
                detailBlade: {
                    template: 'Modules/$(VirtoCommerce.Customer)/Scripts/blades/vendor-detail.tpl.html',
                    metaFields0: [{
                        name: 'name',
                        title: "customer.blades.vendor-detail.labels.name",
                        placeholder: "customer.blades.vendor-detail.placeholders.name",
                        isRequired: true,
                        spanAllColumns: true,
                        valueType: "ShortText"
                    }, {
                        spanAllColumns: true,
                        templateUrl: "status.html"
                    }],
                    metaFields: [{
                        name: 'siteUrl',
                        title: "customer.blades.vendor-detail.labels.site-url",
                        placeholder: "customer.blades.vendor-detail.placeholders.site-url",
                        spanAllColumns: true,
                        valueType: "Url"
                    }, {
                        name: 'logoUrl',
                        title: "customer.blades.vendor-detail.labels.logo-url",
                        placeholder: "customer.blades.vendor-detail.placeholders.logo-url",
                        spanAllColumns: true,
                        valueType: "Url"
                    }, {
                        name: 'groupName',
                        title: "customer.blades.vendor-detail.labels.group-name",
                        placeholder: "customer.blades.vendor-detail.placeholders.group-name",
                        spanAllColumns: true,
                        valueType: "ShortText"
                    }, {
                        name: 'description',
                        title: "customer.blades.vendor-detail.labels.description",
                        placeholder: "customer.blades.vendor-detail.placeholders.description",
                        spanAllColumns: true,
                        valueType: "LongText"
                    }]
                }
            });

            metaFormsService.registerMetaFields('CustomerAddressDetails', [
                {
                    templateUrl: 'description.html',
                    priority: 0
                },
                {
                    templateUrl: 'addressTypeSelector.html',
                    priority: 1
                },
                {
                    name: 'firstName',
                    title: 'customer.blades.address-details.labels.first-name',
                    placeholder: 'customer.blades.address-details.placeholders.first-name',
                    valueType: 'ShortText',
                    isRequired: false,
                    priority: 2
                },
                {
                    name: 'middleName',
                    title: 'customer.blades.address-details.labels.middle-name',
                    placeholder: 'customer.blades.address-details.placeholders.middle-name',
                    valueType: 'ShortText',
                    isRequired: false,
                    priority: 3
                },
                {
                    name: 'lastName',
                    title: 'customer.blades.address-details.labels.last-name',
                    placeholder: 'customer.blades.address-details.placeholders.last-name',
                    valueType: 'ShortText',
                    isRequired: false,
                    priority: 4
                },
                {
                    templateUrl: 'countrySelector.html',
                    priority: 5
                },
                {
                    templateUrl: 'countryRegionSelector.html',
                    priority: 6
                },
                {
                    name: 'city',
                    title: 'customer.blades.address-details.labels.city',
                    placeholder: 'customer.blades.address-details.placeholders.city',
                    valueType: 'ShortText',
                    isRequired: true,
                    priority: 7
                },
                {
                    name: 'line1',
                    title: 'customer.blades.address-details.labels.address1',
                    placeholder: 'customer.blades.address-details.placeholders.address1',
                    valueType: 'ShortText',
                    isRequired: true,
                    priority: 8
                },
                {
                    name: 'line2',
                    title: 'customer.blades.address-details.labels.address2',
                    placeholder: 'customer.blades.address-details.placeholders.address2',
                    valueType: 'ShortText',
                    priority: 9
                },
                {
                    name: 'postalCode',
                    title: 'customer.blades.address-details.labels.zip-code',
                    placeholder: 'customer.blades.address-details.placeholders.zip-code',
                    valueType: 'ShortText',
                    isRequired: true,
                    priority: 10
                },
                {
                    name: 'email',
                    title: 'customer.blades.address-details.labels.email',
                    placeholder: 'customer.blades.address-details.placeholders.email',
                    valueType: 'Email',
                    priority: 11
                },
                {
                    name: 'phone',
                    title: 'customer.blades.address-details.labels.phone',
                    placeholder: 'customer.blades.address-details.placeholders.phone',
                    valueType: 'ShortText',
                    priority: 12
                }
            ]);

            // invite customer metaform
            metaFormsService.registerMetaFields('inviteCustomerDetails', [
                {
                    title: "customer.blades.invite-customers.labels.store",
                    templateUrl: 'invite-stores.html',
                },
                {
                    title: "customer.blades.invite-customers.labels.language",
                    templateUrl: 'invite-languages.html',
                },
                {
                    title: "customer.blades.invite-customers.labels.organization",
                    templateUrl: 'invite-organizations.html',
                },
                {
                    title: "customer.blades.invite-customers.labels.role",
                    templateUrl: 'invite-roles.html',
                },
                {
                    name: 'emailsString',
                    title: "customer.blades.invite-customers.labels.emails",
                    placeholder: "customer.blades.invite-customers.placeholders.emails",
                    spanAllColumns: true,
                    isRequired: true,
                    valueType: "LongText"
                }, 
                {
                    name: 'message',
                    title: "customer.blades.invite-customers.labels.message",
                    placeholder: "customer.blades.invite-customers.placeholders.message",
                    spanAllColumns: true,
                    valueType: "LongText"
                }           
            ]);

            // invite customers toolbar button
            toolbarService.register({
                name: "customer.commands.invite-customers",
                icon: 'fa fa-cubes',
                executeMethod: function (blade) {
                    var selectedStore = blade.currentEntity;

                    var newBlade = {
                        id: 'inviteCustomers',
                        selectedStore: selectedStore,
                        controller: 'virtoCommerce.customerModule.inviteCustomersController',
                        template: 'Modules/$(VirtoCommerce.Customer)/Scripts/blades/invite-customers.html'
                    };

                    bladeNavigationService.showBlade(newBlade);

                },
                canExecuteMethod: function () {
                    return true;
                },
                permission: 'customer:invite',
                index: 10
            }, 'virtoCommerce.storeModule.storeDetailController');

            const lastTimeModificationDate = 1583235535540;
            // predefine search filters for customer search
            predefinedSearchFilters.register(lastTimeModificationDate, 'customerSearchFiltersDate', [
                { name: 'customer.blades.member-list.labels.filter-new' },
                { keyword: 'membertype:Vendor', id: 3, name: 'customer.blades.member-list.labels.filter-vendor' },
                { keyword: 'membertype:Contact', id: 2, name: 'customer.blades.member-list.labels.filter-contact' },
                { keyword: 'membertype:Organization', id: 1, name: 'customer.blades.member-list.labels.filter-organization' }
            ]);

            // Register permission scopes
            var associatedOrganizationsOnlyScope = {
                type: 'AssociatedOrganizationsOnlyScope',
                title: 'Only for associated organizations'
            };
            scopeResolver.register(associatedOrganizationsOnlyScope);

            $rootScope.$on("loginStatusChanged", function (event, authContext) {
                if (authContext.isAuthenticated && (authContext.memberId)) {
                    membersApi.get({ id: authContext.memberId }, function (member) {
                        userProfileIconService.setUserIconUrl(member.id, member.iconUrl);
                    });
                }
            });
        }]);

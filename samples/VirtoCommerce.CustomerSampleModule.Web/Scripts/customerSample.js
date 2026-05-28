//Call this to register our module to main application
var moduleName = "virtoCommerce.customerSampleModule";

if (AppDependencies != undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .run(
        ['virtoCommerce.customerModule.memberTypesResolverService', 'virtoCommerce.customerModule.memberListFilterExtensionService', 'platformWebApp.authService', 'platformWebApp.widgetService', function (memberTypesResolverService, memberListFilterExtensionService, authService, widgetService) {

            // add JobTitle field to Contact detail blade
            var contactInfo = memberTypesResolverService.resolve("Contact");
            contactInfo.detailBlade.metaFields.unshift(
                {
                    name: 'jobTitle',
                    title: "JobTitle",
                    valueType: "ShortText"
                },
                {
                    name: 'webContactId',
                    title: "WebContactId",
                    valueType: "ShortText"
                }
            );

            // register new Supplier member type
            memberTypesResolverService.registerType({
                memberType: 'Supplier',
                description: 'Supplier description',
                fullTypeName: 'virtoCommerce.customerSampleModule.Web.Model.Supplier',
                icon: 'fa fa-truck',
                detailBlade: {
                    template: 'Modules/$(VirtoCommerce.customerSample)/Scripts/blades/supplier-detail.html',
                    metaFields: [{
                        name: 'contractNumber',
                        title: "Contract Number",
                        valueType: "ShortText"
                    }]
                }
            });

            // register a custom filter row in the Members blade's <va-filter-panel>.
            // The partial binds to blade.filter.sampleJobTitle; this descriptor wires
            // that state into hasActiveFilters / clearFilters and contributes a
            // Lucene-style jobtitle:"..." token to the search request, matching the
            // jobTitle metaField added above so the filter exercises the same field
            // it just registered on Contact.
            memberListFilterExtensionService.register({
                id: 'sample-job-title',
                priority: 110,
                templateUrl: 'Modules/$(VirtoCommerce.customerSample)/Scripts/blades/filters/job-title-filter.html',
                init: function (filter) {
                    filter.sampleJobTitle = '';
                },
                hasActiveFilter: function (filter) {
                    return !!filter.sampleJobTitle;
                },
                clear: function (filter) {
                    filter.sampleJobTitle = '';
                },
                appendKeywordTokens: function (filter, tokens) {
                    if (filter.sampleJobTitle) {
                        tokens.push(`jobtitle:"${filter.sampleJobTitle}"`);
                    }
                }
            });

            // Demonstrate dynamic properties management and ability to index them.
            // Shows fixed https://virtocommerce.atlassian.net/browse/PT-214
            var dynamicPropertyWidget = {
                controller: 'platformWebApp.dynamicPropertyWidgetController',
                template: '$(Platform)/Scripts/app/dynamicProperties/widgets/dynamicPropertyWidget.tpl.html',
                isVisible: function (blade) { return !blade.isNew && authService.checkPermission('platform:dynamic_properties:read'); }
            };
            widgetService.registerWidget(dynamicPropertyWidget, 'supplierDetail1');

            var indexWidget = {
                documentType: 'Member',
                controller: 'virtoCommerce.searchModule.indexWidgetController',
                template: 'Modules/$(VirtoCommerce.Search)/Scripts/widgets/index-widget.tpl.html',
                isVisible: function (blade) { return !blade.isNew; }
            };
            widgetService.registerWidget(indexWidget, 'supplierDetail1');
        }]
    );

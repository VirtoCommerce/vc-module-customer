//Call this to register our module to main application
var moduleName = "virtoCommerce.customerSampleModule";

if (AppDependencies != undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .run(
        ['virtoCommerce.customerModule.memberTypesResolverService', 'platformWebApp.authService', 'platformWebApp.widgetService', function (memberTypesResolverService, authService, widgetService) {

            // add JobTitle field to Contact detail blade
            var contactInfo = memberTypesResolverService.resolve("Contact");
            contactInfo.detailBlade.metaFields.unshift({
                name: 'jobTitle',
                title: "JobTitle",
                valueType: "ShortText"
            });

            // register new Supplier member type
            memberTypesResolverService.registerType({
                memberType: 'Supplier',
                description: 'Supplier description',
                fullTypeName: 'virtoCommerce.customerSampleModule.Web.Model.Supplier',
                icon: 'fa fa-truck',
                detailBlade: {
                    template: 'Modules/$(VirtoCommerce.customerSample)/Scripts/blades/supplier-detail.tpl.html',
                    metaFields: [{
                        name: 'contractNumber',
                        title: "Contract Number",
                        valueType: "ShortText"
                    }]
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

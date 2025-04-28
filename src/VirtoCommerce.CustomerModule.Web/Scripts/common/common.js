angular.module('virtoCommerce.customerModule.common', [])
    .run(['platformWebApp.metaFormsService', function (metaFormsService) {
        metaFormsService.registerMetaFields('CustomeraddressDetails', [
            {
                templateUrl: 'discription.html',
                priority: 0
            },
            {
                templateUrl: 'addressTypeSelector.html',
                priority: 1
            },
            {
                name: 'firstName',
                title: 'core.blades.address-detail.labels.first-name',
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
                title: 'core.blades.address-detail.labels.last-name',
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
                title: 'core.blades.address-detail.labels.city',
                valueType: 'ShortText',
                isRequired: true,
                priority: 7
            },
            {
                name: 'line1',
                title: 'core.blades.address-detail.labels.address1',
                valueType: 'ShortText',
                isRequired: true,
                priority: 8
            },
            {
                name: 'line2',
                title: 'core.blades.address-detail.labels.address2',
                valueType: 'ShortText',
                priority: 9
            },
            {
                name: 'postalCode',
                title: 'core.blades.address-detail.labels.zip-code',
                valueType: 'ShortText',
                isRequired: true,
                priority: 10
            },
            {
                name: 'email',
                title: 'core.blades.address-detail.labels.email',
                valueType: 'Email',
                priority: 11
            },
            {
                name: 'phone',
                title: 'core.blades.address-detail.labels.phone',
                valueType: 'ShortText',
                priority: 12
            }
        ]);
    }]);

angular.module('virtoCommerce.customerModule').controller('virtoCommerce.customerModule.vendorDetailController',
    ['$scope', 'platformWebApp.settings', function ($scope, settings) {
        var blade = $scope.blade;

        if (blade.isNew) {
            blade.title = 'customer.blades.vendor-detail.title-new';
            blade.currentEntity = angular.extend({
                seoInfos: []
            }, blade.currentEntity);

            blade.fillDynamicProperties();
        } else {
            blade.subtitle = 'customer.blades.vendor-detail.subtitle';
        }

        $scope.statuseSettings = settings.get({ id: 'Customer.VendorStatuses' }, function (data) {
            if (blade.isNew) {
                blade.currentEntity.status = data.defaultValue;
                blade.origEntity.status = data.defaultValue;
            }
        });
    }]);

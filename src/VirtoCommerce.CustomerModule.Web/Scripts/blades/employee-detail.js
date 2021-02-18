angular.module('virtoCommerce.customerModule').controller('virtoCommerce.customerModule.employeeDetailController',
    ['$scope', 'platformWebApp.common.timeZones', function ($scope, timeZones) {
        var blade = $scope.blade;

        if (blade.isNew) {
            blade.title = 'customer.blades.employee-detail.title-new';
            blade.currentEntity = angular.extend({
                isActive: true,
                organizations: []
            }, blade.currentEntity);

            if (blade.parentBlade.currentEntity.id) {
                blade.currentEntity.organizations.push(blade.parentBlade.currentEntity.id);
            }

            blade.fillDynamicProperties();
        } else {
            blade.subtitle = 'customer.blades.employee-detail.subtitle';
        }

        // datepicker
        blade.datepickers = {};
        blade.today = new Date();

        blade.open = function ($event, which) {
            $event.preventDefault();
            $event.stopPropagation();

            blade.datepickers[which] = true;
        };

        blade.timeZones = timeZones.query();
    }]);

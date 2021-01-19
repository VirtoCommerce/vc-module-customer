angular.module('virtoCommerce.customerModule').controller('virtoCommerce.customerModule.employeeDetailController',
    ['$scope', 'platformWebApp.common.timeZones', 'platformWebApp.settings',
        function ($scope, timeZones, settings) {
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

            $scope.statusSettings = settings.get({ id: 'Customer.EmployeeStatuses' }, function (data) {
                if (blade.isNew) {
                    blade.currentEntity.status = data.defaultValue;
                    blade.origEntity.status = data.defaultValue;
                }
            });

            // datepicker
            $scope.datepickers = {};
            $scope.today = new Date();

            $scope.open = function ($event, which) {
                $event.preventDefault();
                $event.stopPropagation();

                $scope.datepickers[which] = true;
            };

            $scope.timeZones = timeZones.query();
        }]);

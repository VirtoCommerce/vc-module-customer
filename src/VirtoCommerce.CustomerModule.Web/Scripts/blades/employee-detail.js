angular.module('virtoCommerce.customerModule').controller('virtoCommerce.customerModule.employeeDetailController',
    ['$scope', 'platformWebApp.common.timeZones', 'platformWebApp.settings', 'platformWebApp.bladeNavigationService',
        function ($scope, timeZones, settings, bladeNavigationService) {
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

            $scope.statuseSettings = settings.get({ id: 'Customer.EmployeeStatuses' }, function (data) {
                if (blade.isNew) {
                    blade.currentEntity.status = data.defaultValue;
                    blade.origEntity.status = data.defaultValue;
                }
            });

            blade.openStatusSettingManagement = function () {
                var newBlade = {
                    id: 'settingDetailChild',
                    isApiSave: true,
                    currentEntityId: 'Customer.EmployeeStatuses',
                    controller: 'platformWebApp.settingDictionaryController',
                    template: '$(Platform)/Scripts/app/settings/blades/setting-dictionary.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };

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

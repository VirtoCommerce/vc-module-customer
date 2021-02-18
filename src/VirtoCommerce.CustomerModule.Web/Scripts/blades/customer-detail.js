angular.module('virtoCommerce.customerModule').controller('virtoCommerce.customerModule.customerDetailController',
    ['$scope', 'platformWebApp.common.timeZones', 'platformWebApp.settings', 'platformWebApp.bladeNavigationService',
        function ($scope, timeZones, settings, bladeNavigationService) {
            var blade = $scope.blade;

            if (blade.isNew) {
                blade.title = 'customer.blades.contact-detail.title-new';
                blade.currentEntity = angular.extend({
                    organizations: []
                }, blade.currentEntity);

                if (blade.parentBlade.currentEntity.id) {
                    blade.currentEntity.organizations.push(blade.parentBlade.currentEntity.id);
                }

                blade.fillDynamicProperties();
            } else {
                blade.subtitle = 'customer.blades.contact-detail.subtitle';
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
            blade.groups = settings.getValues({ id: 'Customer.MemberGroups' });

            blade.openGroupsDictionarySettingManagement = function () {
                var newBlade = {
                    id: 'settingDetailChild',
                    isApiSave: true,
                    currentEntityId: 'Customer.MemberGroups',
                    parentRefresh: function (data) { $scope.groups = data; },
                    controller: 'platformWebApp.settingDictionaryController',
                    template: '$(Platform)/Scripts/app/settings/blades/setting-dictionary.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };

            blade.fetchCustomerOrganizations = function ($select) {
                $scope.fetchOrganizations($select).then(function () {
                    blade.customerOrganizations = angular.copy($scope.organizations);
                });
            }

            blade.fetchAssociatedOrganizations = function ($select) {
                $scope.fetchOrganizations($select).then(function () {
                    blade.associatedOrganizations = angular.copy($scope.organizations);
                });
            }

            blade.fetchNextCustomerOrganizations = function ($select) {
                $scope.fetchNextOrganizations($select).then(function () {
                    blade.customerOrganizations = angular.copy($scope.organizations);
                });
            }

            blade.fetchNextAssociatedOrganizations = function ($select) {
                $scope.fetchNextOrganizations($select).then(function () {
                    blade.associatedOrganizations = angular.copy($scope.organizations);
                });
            }
        }]);

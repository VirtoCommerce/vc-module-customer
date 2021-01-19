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
            $scope.datepickers = {};
            $scope.today = new Date();

            $scope.open = function ($event, which) {
                $event.preventDefault();
                $event.stopPropagation();

                $scope.datepickers[which] = true;
            };

            $scope.timeZones = timeZones.query();
            $scope.groups = settings.getValues({ id: 'Customer.MemberGroups' });

            $scope.statuseSettings = settings.get({ id: 'Customer.ContactStatuses' }, function (data) {
                if (blade.isNew) {
                    blade.currentEntity.status = data.defaultValue;
                    blade.origEntity.status = data.defaultValue;
                }
            });

            blade.openStatusSettingManagement = function () {
                var newBlade = {
                    id: 'settingDetailChild',
                    isApiSave: true,
                    currentEntityId: 'Customer.ContactStatuses',
                    controller: 'platformWebApp.settingDictionaryController',
                    template: '$(Platform)/Scripts/app/settings/blades/setting-dictionary.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };

            $scope.openGroupsDictionarySettingManagement = function () {
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

            $scope.fetchCustomerOrganizations = function ($select) {
                $scope.fetchOrganizations($select).then(function () {
                    $scope.customerOrganizations = angular.copy($scope.organizations);
                });
            }

            $scope.fetchAssociatedOrganizations = function ($select) {
                $scope.fetchOrganizations($select).then(function () {
                    $scope.associatedOrganizations = angular.copy($scope.organizations);
                });
            }

            $scope.fetchNextCustomerOrganizations = function ($select) {
                $scope.fetchNextOrganizations($select).then(function () {
                    $scope.customerOrganizations = angular.copy($scope.organizations);
                });
            }

            $scope.fetchNextAssociatedOrganizations = function ($select) {
                $scope.fetchNextOrganizations($select).then(function () {
                    $scope.associatedOrganizations = angular.copy($scope.organizations);
                });
            }
        }]);

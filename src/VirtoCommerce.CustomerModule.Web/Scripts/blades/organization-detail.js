angular.module('virtoCommerce.customerModule').controller('virtoCommerce.customerModule.organizationDetailController',
    ['$scope', 'platformWebApp.settings', 'platformWebApp.bladeNavigationService', 'virtoCommerce.customerModule.rolesPickerService',
        function ($scope, settings, bladeNavigationService, rolesPickerService) {
            var blade = $scope.blade;

            if (blade.isNew) {
                blade.title = 'customer.blades.organization-detail.title-new';
                blade.currentEntity.parentId = blade.parentBlade.currentEntity.id;

                blade.fillDynamicProperties();
            } else {
                blade.subtitle = 'customer.blades.organization-detail.subtitle';
            }

            blade.groups = settings.getValues({ id: 'Customer.MemberGroups' });
            blade.availableRoles = [];

            var rolesPicker = rolesPickerService.create({
                whitelistSettingId: 'Customer.OrganizationRolesWhitelist',
                getSelectedRoles: function () { return blade.currentEntity.roles; },
                onAvailableRolesChanged: function (list) { blade.availableRoles = list; }
            });

            blade.refreshRoles = rolesPicker.refresh;

            $scope.$watchCollection('blade.currentEntity.roles', function (newRoles) {
                if (!newRoles) {
                    return;
                }

                rolesPicker.normalizeSelected(newRoles);
                rolesPicker.syncAvailableRoles();
            });

            blade.openGroupsDictionarySettingManagement = function () {
                var newBlade = {
                    id: 'settingDetailChild',
                    isApiSave: true,
                    currentEntityId: 'Customer.MemberGroups',
                    parentRefresh: function (data) { blade.groups = data; },
                    controller: 'platformWebApp.settingDictionaryController',
                    template: '$(Platform)/Scripts/app/settings/blades/setting-dictionary.tpl.html'
                };

                bladeNavigationService.showBlade(newBlade, blade);
            };
        }]);

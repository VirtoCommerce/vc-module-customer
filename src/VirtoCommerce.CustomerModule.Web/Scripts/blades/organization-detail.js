angular.module('virtoCommerce.customerModule').controller('virtoCommerce.customerModule.organizationDetailController',
    ['$scope', 'platformWebApp.settings', 'platformWebApp.bladeNavigationService', 'platformWebApp.roles',
        function ($scope, settings, bladeNavigationService, roles) {
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

            var orgRolesWhitelist = settings.getValues({ id: 'Customer.OrganizationRolesWhitelist' });

            blade.refreshRoles = function (keyword) {
                roles.search({ keyword: keyword || '', take: 20 }).$promise.then(function (data) {
                    var allRoles = data.results || [];
                    blade.availableRoles = orgRolesWhitelist.length
                        ? allRoles.filter(function (r) { return orgRolesWhitelist.indexOf(r.name) !== -1; })
                        : allRoles;
                });
            };

            blade.isRoleAvailable = function (role) {
                var selected = (blade.currentEntity.roles || []).map(function (r) { return r.roleId || r.id; });
                return selected.indexOf(role.id) === -1;
            };

            // Normalize roles added from the platform roles picker ({ id, name }) into OrganizationRole shape ({ roleId, roleName }).
            // Replaces the item reference rather than mutating in-place to avoid corrupting the shared object in availableRoles.
            $scope.$watchCollection('blade.currentEntity.roles', function (newRoles) {
                if (!newRoles) return;
                for (var i = 0; i < newRoles.length; i++) {
                    var r = newRoles[i];
                    if (!r.roleId && r.id) {
                        newRoles[i] = { roleId: r.id, roleName: r.name };
                    }
                }
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

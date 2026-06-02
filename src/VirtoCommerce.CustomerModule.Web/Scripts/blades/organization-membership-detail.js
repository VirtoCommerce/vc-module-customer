angular.module('virtoCommerce.customerModule')
    .controller('virtoCommerce.customerModule.organizationMembershipDetailController',
        ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService',
         'virtoCommerce.customerModule.organizationMemberships', 'virtoCommerce.customerModule.organizations',
         'platformWebApp.roles',
        function ($scope, bladeNavigationService, dialogService,
                  organizationMemberships, organizations, roles) {
            var blade = $scope.blade;
            blade.updatePermission = 'customer:update';

            // Available roles loaded into array so ui-select can bind synchronously
            blade.availableRoles = [];

            blade.refresh = function () {
                if (blade.isNew) {
                    blade.origEntity = {};
                    blade.isLoading = false;
                    return;
                }
                blade.isLoading = true;
                organizationMemberships.getById({ id: blade.currentEntity.id }, function (data) {
                    blade.currentEntity = angular.copy(data);
                    blade.origEntity = data;
                    blade.isLoading = false;
                }, function () {
                    blade.isLoading = false;
                });
            };

            // Called by ui-select refresh attribute — populates blade.availableRoles
            blade.refreshRoles = function (keyword) {
                roles.search({ keyword: keyword || '', take: 20 }).$promise.then(function (data) {
                    blade.availableRoles = data.results || [];
                });
            };

            $scope.isRoleAvailable = function (role) {
                var selectedIds = (blade.currentEntity.roles || []).map(function (r) { return r.roleId || r.id; });
                return selectedIds.indexOf(role.id) === -1;
            };

            // Load available organisations for the org picker (ui-scroll-drop-down)
            blade.fetchOrganizations = function (criteria) {
                criteria = criteria || {};
                criteria.take = criteria.take || 20;
                return organizations.search(criteria);
            };

            $scope.setForm = function (form) {
                $scope.formScope = form;
            };

            function isDirty() {
                return !angular.equals(blade.currentEntity, blade.origEntity) && blade.hasUpdatePermission();
            }

            function canSave() {
                return isDirty() && $scope.formScope && $scope.formScope.$valid;
            }

            // Normalise roles: ui-select puts platform role objects {id, name} into the array,
            // but the server expects {roleId, roleName}. Map both formats before saving.
            function normalisedPayload() {
                var payload = angular.copy(blade.currentEntity);
                payload.roles = (payload.roles || []).map(function (r) {
                    return {
                        id: r.membershipId ? r.id : undefined,
                        membershipId: r.membershipId,
                        roleId: r.roleId || r.id,
                        roleName: r.roleName || r.name
                    };
                });
                return payload;
            }

            $scope.saveChanges = function () {
                // Client-side uniqueness check — prevents duplicate user+org pairs
                if (blade.isNew && blade.currentEntity.organizationId) {
                    var existing = blade.parentBlade.currentEntities || [];
                    var duplicate = _.some(existing, function (m) {
                        return m.organizationId === blade.currentEntity.organizationId;
                    });
                    if (duplicate) {
                        dialogService.showNotificationDialog({
                            id: 'duplicateMembership',
                            title: 'customer.dialogs.membership-duplicate.title',
                            message: 'customer.dialogs.membership-duplicate.message'
                        });
                        return;
                    }
                }

                blade.isLoading = true;
                if (blade.isNew) {
                    organizationMemberships.create(
                        { userId: blade.userId },
                        normalisedPayload(),
                        function (result) {
                            blade.isLoading = false;
                            blade.isNew = false;
                            blade.currentEntity = result;
                            blade.origEntity = angular.copy(result);
                            blade.parentBlade.refresh();
                        },
                        function () { blade.isLoading = false; }
                    );
                } else {
                    organizationMemberships.update(
                        { id: blade.currentEntity.id },
                        normalisedPayload(),
                        function (result) {
                            blade.isLoading = false;
                            blade.currentEntity = angular.copy(result);
                            blade.origEntity = angular.copy(result);
                            blade.parentBlade.refresh();
                        },
                        function () { blade.isLoading = false; }
                    );
                }
            };

            $scope.lockMembership = function () {
                blade.isLoading = true;
                organizationMemberships.lock(
                    { id: blade.currentEntity.id },
                    {},
                    function (result) {
                        blade.currentEntity = result;
                        blade.origEntity = angular.copy(result);
                        blade.isLoading = false;
                        blade.parentBlade.refresh();
                    },
                    function () { blade.isLoading = false; }
                );
            };

            $scope.unlockMembership = function () {
                blade.isLoading = true;
                organizationMemberships.unlock(
                    { id: blade.currentEntity.id },
                    {},
                    function (result) {
                        blade.currentEntity = result;
                        blade.origEntity = angular.copy(result);
                        blade.isLoading = false;
                        blade.parentBlade.refresh();
                    },
                    function () { blade.isLoading = false; }
                );
            };

            blade.toolbarCommands = [
                {
                    name: 'platform.commands.save',
                    icon: 'fas fa-save',
                    executeMethod: $scope.saveChanges,
                    canExecuteMethod: canSave,
                    permission: blade.updatePermission
                },
                {
                    name: 'platform.commands.reset',
                    icon: 'fa fa-undo',
                    executeMethod: function () {
                        blade.currentEntity = angular.copy(blade.origEntity);
                    },
                    canExecuteMethod: isDirty
                },
                {
                    name: 'customer.commands.lock',
                    icon: 'fas fa-lock',
                    executeMethod: $scope.lockMembership,
                    canExecuteMethod: function () {
                        return !blade.isNew && !blade.currentEntity.isLocked;
                    },
                    permission: blade.updatePermission
                },
                {
                    name: 'customer.commands.unlock',
                    icon: 'fas fa-lock-open',
                    executeMethod: $scope.unlockMembership,
                    canExecuteMethod: function () {
                        return !blade.isNew && blade.currentEntity.isLocked;
                    },
                    permission: blade.updatePermission
                }
            ];

            blade.refresh();
        }]);

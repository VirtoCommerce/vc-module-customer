angular.module('virtoCommerce.customerModule')
    .controller('virtoCommerce.customerModule.organizationMembershipDetailController',
        ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService',
         'virtoCommerce.customerModule.organizationMemberships', 'virtoCommerce.customerModule.organizations',
         'virtoCommerce.customerModule.rolesPickerService',
        function ($scope, bladeNavigationService, dialogService,
                  organizationMemberships, organizations, rolesPickerService) {
            var blade = $scope.blade;
            blade.updatePermission = 'customer:update';

            blade.metaFields = [
                {
                    name: 'organizationId',
                    templateUrl: 'orgMembership.organization.html'
                },
                {
                    name: 'roles',
                    templateUrl: 'orgMembership.roles.html'
                },
                {
                    name: 'status',
                    templateUrl: 'orgMembership.status.html'
                },
                {
                    name: 'lockoutEnd',
                    templateUrl: 'orgMembership.lockoutEnd.html'
                }
            ];

            blade.availableRoles = [];
            blade.datepickers = {};

            blade.refresh = function () {
                if (blade.isNew) {
                    blade.origEntity = {};
                    blade.isLoading = false;

                    return;
                }
                blade.isLoading = true;
                organizationMemberships.getById({ id: blade.currentEntity.id }, function (data) {
                    blade.currentEntity = angular.copy(data);
                    blade.origEntity = angular.copy(data);
                    blade.isLoading = false;
                }, function () {
                    blade.isLoading = false;
                });
            };

            var rolesPicker = rolesPickerService.create({
                whitelistSettingId: 'Customer.MembershipRolesWhitelist',
                getSelectedRoles: function () { return blade.currentEntity.roles; },
                onAvailableRolesChanged: function (list) { blade.availableRoles = list; }
            });

            blade.refreshRoles = rolesPicker.refresh;

            $scope.$watchCollection('blade.currentEntity.roles', function (newRoles) {
                if (!newRoles) {
                    return;
                }

                rolesPicker.syncAvailableRoles();
            });

            blade.fetchOrganizations = function (criteria) {
                criteria = criteria || {};
                criteria.take = criteria.take || 20;

                var searchResult = organizations.search(criteria);
                searchResult.$promise.then(function () {
                    var usedOrgIds = _.compact(_.pluck(blade.parentBlade.currentEntities || [], 'organizationId'));
                    searchResult.results = _.reject(searchResult.results || [], function (org) {
                        return _.contains(usedOrgIds, org.id);
                    });
                });
                return searchResult;
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

                            if (blade.parentBlade.parentBlade && blade.parentBlade.parentBlade.refresh) {
                                blade.parentBlade.parentBlade.refresh();
                            }
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

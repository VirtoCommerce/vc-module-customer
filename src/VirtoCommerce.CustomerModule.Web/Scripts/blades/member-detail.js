angular.module('virtoCommerce.customerModule').controller('virtoCommerce.customerModule.memberDetailController',
    ['$rootScope', '$scope', '$timeout', 'platformWebApp.bladeNavigationService', 'virtoCommerce.customerModule.members', 'platformWebApp.dynamicProperties.api', 'virtoCommerce.customerModule.organizations', 'platformWebApp.settings',
        function ($rootScope, $scope, $timeout, bladeNavigationService, members, dynamicPropertiesApi, organizationsResource, settings) {
            var blade = $scope.blade;
            blade.updatePermission = 'customer:update';
            blade.currentEntityId = blade.currentEntity.id;

            blade.currentOrganizations = [];
            blade.currentOrganizationsReloaded = true;

            blade.refresh = function (parentRefresh) {
                if (blade.isNew) {
                    blade.currentEntity = angular.extend({
                        dynamicProperties: [],
                        addresses: [],
                        phones: [],
                        emails: []
                    }, blade.currentEntity);
                } else {
                    blade.isLoading = true;
                    members.get({ id: blade.currentEntity.id }, initializeBlade);

                    if (parentRefresh && blade.parentBlade) {
                        blade.parentBlade.refresh(true);
                    }
                }
            };

            blade.openStatusSettingManagement = function (currentEntityId) {
                var newBlade = {
                    id: 'settingDetailChild',
                    isApiSave: true,
                    currentEntityId: currentEntityId,
                    controller: 'platformWebApp.settingDictionaryController',
                    template: '$(Platform)/Scripts/app/settings/blades/setting-dictionary.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };

            blade.statuses = [];
            blade.loadStatuses = function (currentEntityId) {
                settings.get({ id: currentEntityId }, function (data) {
                    blade.statuses = data.allowedValues;
                    if (blade.isNew) {
                        blade.currentEntity.status = data.defaultValue;
                        blade.origEntity.status = data.defaultValue;
                    }
                });
            }

            blade.fillDynamicProperties = function () {
                dynamicPropertiesApi.query({ id: blade.memberTypeDefinition.fullTypeName }, function (results) {
                    _.each(results, function (x) {
                        x.displayNames = undefined;
                        x.values = [];
                    });
                    blade.currentEntity.dynamicProperties = results;
                    initializeBlade(blade.currentEntity);
                });
            };

            function initializeBlade(data) {
                blade.currentEntity = angular.copy(data);
                blade.origEntity = data;
                blade.customInitialize();
                blade.isLoading = false;

                if (blade.currentEntity.organizations) {
                    var organizationCriteria = {
                        deepSearch: true,
                        objectIds: blade.currentEntity.organizations,
                        skip: 0,
                        take: blade.currentEntity.organizations.length
                    };

                    organizationsResource.search(organizationCriteria, function (organizationsData) {
                        blade.currentOrganizations = organizationsData.results;
                        reloadCurrentOrganizations();
                    });
                }
            }

            // base function to override as needed
            blade.customInitialize = function () {
                if (!blade.isNew) {
                    blade.title = blade.currentEntity.name;
                }
            };

            function isDirty() {
                return !angular.equals(blade.currentEntity, blade.origEntity) && !blade.isNew && blade.hasUpdatePermission();
            }

            function canSave() {
                return isDirty() && $scope.formScope && $scope.formScope.$valid;
            }

            $scope.saveChanges = function () {
                blade.isLoading = true;

                if (blade.isNew) {
                    members.save(blade.currentEntity,
                        function () {
                            blade.parentBlade && blade.parentBlade.refresh(true);
                            blade.origEntity = blade.currentEntity;
                            $scope.bladeClose();
                        });
                } else {
                    members.update(blade.currentEntity,
                        function () { blade.refresh(true); });
                }
                // We always send this event because we don't know if the icon was really changed (it's a bit overwhelming to track it down)
                $rootScope.$broadcast('memberIconChanged', blade.currentEntity.iconUrl);
                bladeNavigationService.closeChildrenBlades(blade);
            };

            $scope.setForm = function (form) {
                $scope.formScope = form;
            }

            blade.onClose = function (closeCallback) {
                bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, $scope.saveChanges, closeCallback, "customer.dialogs.member-save.title", "customer.dialogs.member-save.message");
            };

            if (!blade.isNew) {
                blade.toolbarCommands = [
                    {
                        name: "platform.commands.save",
                        icon: 'fas fa-save',
                        executeMethod: $scope.saveChanges,
                        canExecuteMethod: canSave,
                        permission: blade.updatePermission
                    },
                    {
                        name: "platform.commands.reset",
                        icon: 'fa fa-undo',
                        executeMethod: function () {
                            angular.copy(blade.origEntity, blade.currentEntity);
                        },
                        canExecuteMethod: isDirty,
                        permission: blade.updatePermission
                    }
                ];
            }

            blade.headIcon = blade.memberTypeDefinition.icon;

            blade.fetchOrganizations = function (criteria) {
                criteria.deepSearch = true;
                return organizationsResource.search(criteria);
            }

            blade.selectOrganization = function (item) {
                blade.currentOrganizations.push(item);

                reloadCurrentOrganizations();
            }

            blade.removeOrganization = function (item) {
                var index = _.findIndex(blade.currentOrganizations, function (x) {
                    return x.id === item.id;
                });

                if (index > -1) {
                    blade.currentOrganizations.splice(index, 1);
                }

                if (item.id === blade.currentEntity.defaultOrganizationId) {
                    blade.currentEntity.defaultOrganizationId = null;
                }

                reloadCurrentOrganizations();
            }

            blade.fetchSelectedOrganizations = function () {
                return blade.currentOrganizations;
            }

            function reloadCurrentOrganizations() {
                blade.currentOrganizationsReloaded = false;
                $timeout(function () {
                    blade.currentOrganizationsReloaded = true;
                }, 0);
            }

            blade.refresh(false);
        }]);

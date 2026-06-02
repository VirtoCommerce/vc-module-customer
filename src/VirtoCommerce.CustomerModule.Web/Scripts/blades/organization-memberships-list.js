angular.module('virtoCommerce.customerModule')
    .controller('virtoCommerce.customerModule.organizationMembershipsListController',
        ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService',
         'platformWebApp.uiGridHelper', 'virtoCommerce.customerModule.organizationMemberships',
        function ($scope, bladeNavigationService, dialogService, uiGridHelper, organizationMemberships) {
            var blade = $scope.blade;
            blade.headIcon = 'fas fa-sitemap';

            $scope.uiGridConstants = uiGridHelper.uiGridConstants;

            blade.refresh = function () {
                blade.isLoading = true;
                organizationMemberships.getByUserId({ userId: blade.userId }, function (data) {
                    blade.currentEntities = data || [];
                    blade.isLoading = false;
                    // Keep contact blade (and widget counter) in sync
                    if (blade.parentBlade && blade.parentBlade.refresh) {
                        blade.parentBlade.refresh();
                    }
                }, function () {
                    blade.isLoading = false;
                });
            };

            $scope.selectNode = function (item) {
                $scope.selectedNodeId = item.id;
                openDetail(item);
            };

            function openDetail(item) {
                var newBlade = {
                    id: 'organizationMembershipDetail',
                    userId: blade.userId,
                    currentEntity: angular.copy(item),
                    isGlobal: item.organizationId === null || item.organizationId === undefined,
                    title: item.organizationId ? item.organizationName : 'customer.blades.organization-membership-detail.title-global',
                    subtitle: 'customer.blades.organization-membership-detail.subtitle',
                    controller: 'virtoCommerce.customerModule.organizationMembershipDetailController',
                    template: 'Modules/$(VirtoCommerce.Customer)/Scripts/blades/organization-membership-detail.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            }

            blade.toolbarCommands = [
                {
                    name: 'platform.commands.refresh',
                    icon: 'fa fa-refresh',
                    executeMethod: blade.refresh,
                    canExecuteMethod: function () { return true; }
                },
                {
                    name: 'platform.commands.add',
                    icon: 'fas fa-plus',
                    executeMethod: function () {
                        var newBlade = {
                            id: 'organizationMembershipDetail',
                            userId: blade.userId,
                            currentEntity: { userId: blade.userId, roles: [], isLocked: false },
                            isNew: true,
                            title: 'customer.blades.organization-membership-detail.title-new',
                            subtitle: 'customer.blades.organization-membership-detail.subtitle',
                            controller: 'virtoCommerce.customerModule.organizationMembershipDetailController',
                            template: 'Modules/$(VirtoCommerce.Customer)/Scripts/blades/organization-membership-detail.tpl.html'
                        };
                        bladeNavigationService.showBlade(newBlade, blade);
                    },
                    canExecuteMethod: function () { return true; },
                    permission: 'customer:update'
                },
                {
                    name: 'platform.commands.delete',
                    icon: 'fas fa-trash-alt',
                    executeMethod: function () {
                        var selected = $scope.gridApi.selection.getSelectedRows();
                        // Filter out global memberships — cannot be deleted
                        var deletable = _.filter(selected, function (x) {
                            return x.organizationId !== null && x.organizationId !== undefined;
                        });

                        if (!deletable.length) {
                            return;
                        }

                        var dialog = {
                            id: 'confirmDeleteMembership',
                            title: 'customer.dialogs.membership-delete.title',
                            message: 'customer.dialogs.membership-delete.message',
                            callback: function (confirmed) {
                                if (confirmed) {
                                    bladeNavigationService.closeChildrenBlades(blade, function () {
                                        blade.isLoading = true;
                                        var ids = _.pluck(deletable, 'id');
                                        organizationMemberships.delete({ ids: ids }, function () {
                                            blade.refresh();
                                        }, function () {
                                            blade.isLoading = false;
                                        });
                                    });
                                }
                            }
                        };
                        dialogService.showConfirmationDialog(dialog);
                    },
                    canExecuteMethod: function () {
                        if (!$scope.gridApi) {
                            return false;
                        }

                        var sel = $scope.gridApi.selection.getSelectedRows();
                        return _.some(sel, function (x) {
                            return x.organizationId !== null && x.organizationId !== undefined;
                        });
                    },
                    permission: 'customer:delete'
                }
            ];

            uiGridHelper.initialize($scope, {
                useExternalSorting: false,
                rowTemplate: 'org-memberships-list.row.html',
                columnDefs: [
                    {
                        name: 'organizationName',
                        displayName: 'customer.blades.organization-memberships-list.labels.organization',
                        minWidth: 200
                    },
                    {
                        name: 'roles',
                        displayName: 'customer.blades.organization-memberships-list.labels.roles',
                        cellTemplate: 'org-memberships-roles.cell.html',
                        enableSorting: false
                    },
                    {
                        name: 'status',
                        displayName: 'customer.blades.organization-memberships-list.labels.status',
                        cellTemplate: 'org-memberships-status.cell.html',
                        width: 90, enableSorting: false
                    },
                    {
                        name: 'type',
                        displayName: 'customer.blades.organization-memberships-list.labels.type',
                        cellTemplate: 'org-memberships-type.cell.html',
                        width: 110, enableSorting: false, visible: false
                    },
                    {
                        name: 'lockoutEnd',
                        displayName: 'customer.blades.organization-memberships-list.labels.lockout-end',
                        cellFilter: 'date:"mediumDate"',
                        width: 140, visible: false
                    }
                ]
            }, function (gridApi) { });

            $scope.formatRoles = function (roles) {
                if (!roles || !roles.length) {
                    return '-';
                }

                return _.pluck(roles, 'roleName').filter(Boolean).join(', ') || '-';
            };

            blade.refresh();
        }]);

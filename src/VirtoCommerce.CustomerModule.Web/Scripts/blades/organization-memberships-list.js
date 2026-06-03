angular.module('virtoCommerce.customerModule')
    .controller('virtoCommerce.customerModule.organizationMembershipsListController',
        ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService',
         'platformWebApp.uiGridHelper', 'virtoCommerce.customerModule.organizationMemberships',
         'platformWebApp.bladeUtils',
        function ($scope, bladeNavigationService, dialogService, uiGridHelper, organizationMemberships, bladeUtils) {
            var blade = $scope.blade;
            blade.headIcon = 'fas fa-sitemap';

            $scope.uiGridConstants = uiGridHelper.uiGridConstants;

            bladeUtils.initializePagination($scope);
            blade.pageSettings = $scope.pageSettings;

            blade.refresh = function () {
                blade.isLoading = true;
                var skip = (blade.pageSettings.currentPage - 1) * blade.pageSettings.itemsPerPageCount;
                organizationMemberships.search(
                    { userId: blade.userId, skip: skip, take: blade.pageSettings.itemsPerPageCount },
                    function (data) {
                        blade.currentEntities = data.results || [];
                        blade.pageSettings.totalItems = data.totalCount || 0;
                        blade.pageSettings.numPages = Math.ceil(blade.pageSettings.totalItems / blade.pageSettings.itemsPerPageCount);
                        blade.isLoading = false;
                    },
                    function () {
                        blade.isLoading = false;
                    }
                );
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
                                    blade.isLoading = true;
                                    var ids = _.pluck(deletable, 'id');
                                    organizationMemberships.delete({ ids: ids }, function () {
                                        bladeNavigationService.closeChildrenBlades(blade, function () {
                                            blade.refresh();

                                            if (blade.parentBlade && blade.parentBlade.refresh) {
                                                blade.parentBlade.refresh();
                                            }
                                        });
                                    }, function () {
                                        blade.isLoading = false;
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
                useExternalPagination: false,
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
                        width: 90,
                        enableSorting: false
                    },
                    {
                        name: 'type',
                        displayName: 'customer.blades.organization-memberships-list.labels.type',
                        cellTemplate: 'org-memberships-type.cell.html',
                        width: 110,
                        enableSorting: false,
                        visible: false
                    },
                    {
                        name: 'lockoutEnd',
                        displayName: 'customer.blades.organization-memberships-list.labels.lockout-end',
                        cellFilter: 'date:"mediumDate"',
                        width: 140,
                        visible: false
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

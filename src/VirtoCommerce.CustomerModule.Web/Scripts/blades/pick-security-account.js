angular.module('virtoCommerce.customerModule')
    .controller('virtoCommerce.customerModule.pickSecurityAccountController',
        ['$scope', 'platformWebApp.accounts', 'platformWebApp.uiGridHelper',
            'platformWebApp.bladeNavigationService', 'platformWebApp.bladeUtils',
            'virtoCommerce.customerModule.members', 'platformWebApp.dialogService',
            function ($scope, accounts, uiGridHelper, bladeNavigationService, bladeUtils, members, dialogService) {
                var blade = $scope.blade;
                blade.headIcon = 'fas fa-key';
                blade.title = 'customer.blades.pick-security-account-list.title';
                blade.subtitle = 'customer.blades.pick-security-account-list.subtitle';

                $scope.uiGridConstants = uiGridHelper.uiGridConstants;
                var filter = $scope.filter = {};

                blade.refresh = function () {
                    blade.isLoading = true;

                    accounts.search({
                        keyword: filter.keyword,
                        sort: uiGridHelper.getSortExpression($scope),
                        skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                        take: $scope.pageSettings.itemsPerPageCount
                    }, function (data) {
                        blade.isLoading = false;

                        $scope.pageSettings.totalItems = data.totalCount;
                        blade.currentEntities = data.results;
                    }, function (error) {
                        bladeNavigationService.setError('Error ' + error.status, blade);
                    });
                };

                blade.selectNode = function (node) {
                    $scope.selectedNodeId = node.id;

                    var newBlade = {
                        id: 'listItemChild',
                        controller: 'platformWebApp.accountDetailController',
                        template: '$(Platform)/Scripts/app/security/blades/account-detail.tpl.html',
                        data: node,
                        title: node.userName,
                        subtitle: blade.subtitle,
                    };

                    bladeNavigationService.showBlade(newBlade, blade);
                };

                function isRowSelectable(row) {
                    return row.entity.memberId !== blade.currentEntity.memberId;
                }

                function rowSelectionChanged(row) {
                    if (row.isSelected && row.entity.memberId) {
                        members.get({ id: row.entity.memberId }, function (member) {
                            if (member) {
                                const dialog = {
                                    id: 'confirmLinkAccount',
                                    title: 'customer.dialogs.confirm-account-link.title',
                                    message: 'customer.dialogs.confirm-account-link.message',
                                    messageValues: { memberName: member.name },
                                    callback: function (isConfirmed) {
                                        row.isSelected = isConfirmed;
                                    },
                                    callbackOnDismiss: function (_) {
                                        row.isSelected = false;
                                    },
                                };
                                dialogService.showConfirmationDialog(dialog);
                            }
                        });
                    }
                }

                function linkAccounts(selectedAccounts) {
                    blade.isLoading = true;

                    const updatePromises = selectedAccounts.map(account => {
                        return new Promise((resolve, reject) => {
                            account.memberId = blade.currentEntity.memberId;
                            accounts.update(account, function (result) {
                                if (result.succeeded) {
                                    resolve();
                                } else {
                                    bladeNavigationService.setError(result.errors.join(), blade);
                                    reject(result.errors);
                                }
                            });
                        });
                    });

                    Promise.all(updatePromises)
                        .then(() => {
                            blade.parentBlade.refresh();
                            $scope.bladeClose();
                        })
                        .finally(() => {
                            blade.isLoading = false;
                        });
                }

                blade.toolbarCommands = [
                    {
                        name: "platform.commands.save", icon: 'fas fa-save',
                        executeMethod: function () { linkAccounts($scope.gridApi.selection.getSelectedRows()); },
                        canExecuteMethod: () => $scope.gridApi?.selection?.getSelectedRows()?.length > 0
                    },
                    {
                        name: "platform.commands.refresh", icon: 'fa fa-refresh',
                        executeMethod: blade.refresh,
                        canExecuteMethod: function () {
                            return true;
                        }
                    }
                ];

                filter.criteriaChanged = function () {
                    if ($scope.pageSettings.currentPage > 1) {
                        $scope.pageSettings.currentPage = 1;
                    } else {
                        blade.refresh();
                    }
                };

                // ui-grid
                $scope.setGridOptions = function (gridOptions) {
                    gridOptions.enableSelectAll = false;
                    gridOptions.isRowSelectable = isRowSelectable;

                    uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                        uiGridHelper.bindRefreshOnSortChanged($scope);
                        gridApi.selection.on.rowSelectionChanged($scope, rowSelectionChanged);
                    });

                    bladeUtils.initializePagination($scope);
                };
            }]);

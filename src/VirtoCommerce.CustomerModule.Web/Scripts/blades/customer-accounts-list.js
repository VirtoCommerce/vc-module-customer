angular.module('virtoCommerce.customerModule')
.controller('virtoCommerce.customerModule.customerAccountsListController', ['$scope', 'platformWebApp.dialogService', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeNavigationService', 'filterFilter', 'platformWebApp.accounts',
function ($scope, dialogService, uiGridHelper, bladeNavigationService, filterFilter, accounts) {
    var blade = $scope.blade;
    blade.headIcon = 'fas fa-key';

    $scope.uiGridConstants = uiGridHelper.uiGridConstants;

    blade.refresh = function () {
        blade.isLoading = true;
        blade.parentBlade.refresh();
    };

    function initializeBlade(data) {
        blade.memberId = data.id;
        blade.currentEntities = angular.copy(data.securityAccounts);
        blade.origEntity = data.securityAccounts;
        blade.isLoading = false;
    }

    blade.selectNode = function (node) {
        if (bladeNavigationService.checkPermission('platform:security:read')) {
            $scope.selectedNodeId = node.userName;

            var newBlade = {
                id: 'listItemChild',
                data: node,
                title: node.userName,
                fromContact: true,
                subtitle: blade.subtitle,
                controller: 'platformWebApp.accountDetailController',
                template: '$(Platform)/Scripts/app/security/blades/account-detail.tpl.html'
            };

            bladeNavigationService.showBlade(newBlade, blade);
        } else {
            bladeNavigationService.setError('Insufficient permission', blade);
        }
    };

    function openNewAccountWizard(store) {
        var newBlade = {
            id: 'newAccountWizard',
            currentEntity: { roles: [], userType: 'Customer', storeId: store.id, memberId: blade.memberId },
            title: 'platform.blades.account-detail.title-new',
            subtitle: blade.subtitle,
            controller: 'platformWebApp.newAccountWizardController',
            template: '$(Platform)/Scripts/app/security/wizards/newAccount/new-account-wizard.tpl.html'
        };
        bladeNavigationService.showBlade(newBlade, blade);
    }

    $scope.deleteList = function (selection) {
        var dialog = {
            id: "confirmDeleteItem",
            title: "platform.dialogs.account-delete.title",
            message: "platform.dialogs.account-delete.message",
            callback: function (remove) {
                if (remove) {
                    bladeNavigationService.closeChildrenBlades(blade, function () {
                        var userNames = _.pluck(selection, 'userName');
                        accounts.remove({ names: userNames }, blade.refresh);
                    });
                }
            }
        };
        dialogService.showWarningDialog(dialog);
    };

    $scope.unlinkAccounts = function (selectedAccounts) {
        var dialog = {
            id: "confirmUnlinkAccount",
            title: "customer.dialogs.confirm-account-unlink.title",
            message: "customer.dialogs.confirm-account-unlink.message",
            callback: function (isConfirmed) {
                if (isConfirmed) {
                    bladeNavigationService.closeChildrenBlades(blade, function () {
                        blade.isLoading = true;

                        const updatePromises = selectedAccounts.map(account => {
                            return new Promise((resolve, reject) => {
                                account.memberId = null;
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
                                blade.refresh();
                            })
                            .finally(() => {
                                blade.isLoading = false;
                            });
                    });
                }
            }
        };
        dialogService.showConfirmationDialog(dialog);
    };

    blade.toolbarCommands = [
        {
            name: "platform.commands.add",
            icon: 'fas fa-plus',
            executeMethod: function () {
                bladeNavigationService.closeChildrenBlades(blade, function () {
                    var newBlade = {
                        id: 'pickStoreList',
                        title: 'customer.blades.pick-store-list.title',
                        subtitle: 'customer.blades.pick-store-list.subtitle',
                        onAfterNodeSelected: openNewAccountWizard,
                        controller: 'virtoCommerce.customerModule.pickStoreListController',
                        template: 'Modules/$(VirtoCommerce.Customer)/Scripts/blades/pick-store-list.tpl.html'
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                });
            },
            canExecuteMethod: function () { return true; },
            permission: 'platform:security:create'
        },
        {
            name: "customer.blades.customer-accounts-list.commands.link",
            icon: 'fas fa-link',
            executeMethod: function () {
                bladeNavigationService.closeChildrenBlades(blade, function () {
                    var newBlade = {
                        id: 'pickAccountList',
                        controller: 'virtoCommerce.customerModule.pickSecurityAccountController',
                        template: '$(Platform)/Scripts/app/security/blades/account-list.tpl.html',
                        currentEntity: { memberId: blade.memberId },
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                });
            },
            canExecuteMethod: function () { return true; },
            permission: 'platform:security:update'
        },
        {
            name: "customer.blades.customer-accounts-list.commands.unlink",
            icon: 'fas fa-unlink',
            executeMethod: function () { $scope.unlinkAccounts($scope.gridApi.selection.getSelectedRows()); },
            canExecuteMethod: function () {
                return $scope.gridApi && $scope.gridApi.selection.getSelectedRows().length > 0;
            },
            permission: 'platform:security:update'
        },
        {
            name: "platform.commands.delete",
            icon: 'fas fa-trash-alt',
            executeMethod: function () { $scope.deleteList($scope.gridApi.selection.getSelectedRows()); },
            canExecuteMethod: function () {
                return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
            },
            permission: 'platform:security:delete'
        }
    ];

    // ui-grid
    $scope.setGridOptions = function (gridOptions) {
        uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
            gridApi.grid.registerRowsProcessor($scope.singleFilter, 90);
        });
    };

    $scope.singleFilter = function (renderableRows) {
        var visibleCount = 0;
        renderableRows.forEach(function (row) {
            row.visible = _.any(filterFilter([row.entity], blade.searchText));
            if (row.visible) visibleCount++;
        });

        $scope.filteredEntitiesCount = visibleCount;
        return renderableRows;
    };

    $scope.$watch('blade.parentBlade.currentEntity', initializeBlade);

    // on load:
    // $scope.$watch('blade.parentBlade.currentEntity.securityAccounts' gets fired    
}]);

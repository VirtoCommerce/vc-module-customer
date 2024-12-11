angular.module('virtoCommerce.customerModule')
    .controller('virtoCommerce.customerModule.pickSecurityAccountController',
        ['$scope', 'platformWebApp.accounts', 'platformWebApp.uiGridHelper',
            'platformWebApp.bladeNavigationService', 'platformWebApp.bladeUtils',
            'virtoCommerce.customerModule.members', 'platformWebApp.dialogService',
            function ($scope, accounts, uiGridHelper, bladeNavigationService, bladeUtils, members, dialogService) {
    $scope.uiGridConstants = uiGridHelper.uiGridConstants;
    var blade = $scope.blade;
    var filter = $scope.filter = {};


    blade.title = 'customer.blades.pick-security-account-list.title';
    blade.subtitle = 'customer.blades.pick-security-account-list.subtitle';

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
        const nodeMemberId = node.memberId;

        if (nodeMemberId) {
            members.get({ id: nodeMemberId }, function (member) {
                if (member?.name) {
                    const dialog = {
                        id: 'confirmLinkAccount',
                        title: 'customer.blades.pick-security-account-list.title',
                        message: `The selected account is already associated with ${member.name}. Do you want to proceed?`,
                        callback: function (link) {
                            if (link) {
                                $scope.gridApi.selection.selectRow(node);
                            } else {
                                $scope.gridApi.selection.unSelectRow(node);
                            }
                        }
                    }
                    dialogService.showConfirmationDialog(dialog);
                }
                else {
                    $scope.gridApi.selection.selectRow(node);
                }
            });
        }
    };


    blade.linkAccount = function () {
        blade.isLoading = true;

        const selectedNodes = $scope.gridApi.selection.getSelectedRows();
        const updatePromises = selectedNodes.map(node => {
            return new Promise((resolve, reject) => {
                node.memberId = blade.currentEntity.memberId;
                accounts.update(node, function (result) {
                    if (result.succeeded) {
                        resolve();
                    } else {
                        bladeNavigationService.setError(result.errors.join(), blade);
                        reject(result.errors);
                    }
                });
            });
        });

        Promise.all(updatePromises).then(() => {
            blade.parentBlade.refresh();
            $scope.bladeClose();
        }).finally(() => {
            blade.isLoading = false;
        });
    };

    blade.headIcon = 'fas fa-key';

    blade.toolbarCommands = [
        {
            name: "platform.commands.save", icon: 'fas fa-save',
            executeMethod: blade.linkAccount,
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
        uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
            uiGridHelper.bindRefreshOnSortChanged($scope);
        });
        bladeUtils.initializePagination($scope);
    };
}]);

angular.module('virtoCommerce.customerModule')
    .controller('virtoCommerce.customerModule.pickSecurityAccountController',
        ['$scope', 'platformWebApp.accounts', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeNavigationService', 'platformWebApp.bladeUtils',
function ($scope, accounts, uiGridHelper, bladeNavigationService, bladeUtils) {
    $scope.uiGridConstants = uiGridHelper.uiGridConstants;
    var blade = $scope.blade;
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
        node.memberId = blade.currentEntity.memberId;
        node.storeId = blade.currentEntity.storeId;

        accounts.update(node, function (result) {
            if (result.succeeded) {
                blade.parentBlade.refresh();
            }
            else {
                bladeNavigationService.setError(result.errors.join(), blade);
            }
        });
    };

    blade.headIcon = 'fas fa-key';

    blade.toolbarCommands = [
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

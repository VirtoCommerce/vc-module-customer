angular.module('virtoCommerce.customerModule')
    .controller('virtoCommerce.customerModule.organizationAssetsWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
    var blade = $scope.blade;

    $scope.openBlade = function () {
        var newBlade = {
            id: "organizationAssetList",
            subtitle: blade.title,
            controller: 'virtoCommerce.assetsModule.assetListController',
            template: 'Modules/$(VirtoCommerce.Assets)/Scripts/blades/asset-list.tpl.html',
            currentEntity: { url: '/organizations/' + blade.currentEntity.id }
        };

        bladeNavigationService.showBlade(newBlade, blade);
    };
}]);

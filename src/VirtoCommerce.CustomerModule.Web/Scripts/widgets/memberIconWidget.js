angular.module('platformWebApp')
    .controller('virtoCommerce.customerModule.memberIconWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
        var blade = $scope.blade;
        
        $scope.openBlade = function () {
            var newBlade = {
                id: 'memberIcon',
                currentEntity: blade.currentEntity,
                saveImmediately: blade.saveIconImmediately,
                controller: 'virtoCommerce.customerModule.memberIconController',
                template: 'Modules/$(VirtoCommerce.Customer)/Scripts/blades/member-icon.tpl.html'
            };
            
            bladeNavigationService.showBlade(newBlade, blade);
        };
}]);

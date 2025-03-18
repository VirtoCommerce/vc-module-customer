angular.module('platformWebApp')
    .controller('virtoCommerce.customerModule.memberIconWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
        var blade = $scope.blade;

        $scope.openBlade = function () {
            var newBlade = {
                id: 'memberIcon',
                originalEntity: blade.currentEntity,
                memberId: blade.currentMemberId,
                controller: 'virtoCommerce.customerModule.memberIconController',
                template: 'Modules/$(VirtoCommerce.Customer)/Scripts/blades/member-icon.tpl.html'
            };
            
            bladeNavigationService.showBlade(newBlade, blade);
        };
    }]);

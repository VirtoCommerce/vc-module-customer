angular.module('virtoCommerce.customerModule')
.controller('virtoCommerce.customerModule.organizationDetailController', ['$scope', function ($scope) {
    var blade = $scope.blade;

    if (blade.isNew) {
        blade.title = 'customer.blades.organization-detail.title-new';
        blade.currentEntity.parentId = blade.parentBlade.currentEntity.id;

        blade.fillDynamicProperties();
    } else {
        blade.title = blade.currentEntity.name;
        blade.subtitle = 'customer.blades.organization-detail.subtitle';
    }
}]);
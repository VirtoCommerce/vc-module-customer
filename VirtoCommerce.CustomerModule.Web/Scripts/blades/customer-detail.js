angular.module('virtoCommerce.customerModule')
.controller('virtoCommerce.customerModule.customerDetailController', ['$scope', 'virtoCommerce.customerModule.organizations', 'platformWebApp.common.timeZones', 'platformWebApp.settings', 'platformWebApp.bladeNavigationService', function ($scope, organizations, timeZones, settings, bladeNavigationService) {
    var blade = $scope.blade;

    if (blade.isNew) {
        blade.title = 'customer.blades.contact-detail.title-new';
        blade.currentEntity = angular.extend({
            organizations: []
        }, blade.currentEntity);

        if (blade.parentBlade.currentEntity.id) {
            blade.currentEntity.organizations.push(blade.parentBlade.currentEntity.id);
        }

        blade.fillDynamicProperties();
    } else {
        blade.subtitle = 'customer.blades.contact-detail.subtitle';
    }

    // datepicker
    $scope.datepickers = {}
    $scope.today = new Date();

    $scope.open = function ($event, which) {
        $event.preventDefault();
        $event.stopPropagation();

        $scope.datepickers[which] = true;
    };

    $scope.organizations = organizations.query();
    $scope.timeZones = timeZones.query();
    $scope.groups = settings.getValues({ id: 'Customer.MemberGroups' });

    $scope.openGroupsDictionarySettingManagement = function () {
        var newBlade = {
            id: 'settingDetailChild',
            isApiSave: true,
            currentEntityId: 'Customer.MemberGroups',
            parentRefresh: function (data) { $scope.groups = data; },
            controller: 'platformWebApp.settingDictionaryController',
            template: '$(Platform)/Scripts/app/settings/blades/setting-dictionary.tpl.html'
        };
        bladeNavigationService.showBlade(newBlade, blade);
    };


}]);
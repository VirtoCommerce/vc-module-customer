angular.module('virtoCommerce.customerModule')
.controller('virtoCommerce.customerModule.customerDetailController', ['$scope', 'virtoCommerce.customerModule.members', 'platformWebApp.common.timeZones', 'platformWebApp.settings', 'platformWebApp.bladeNavigationService', function ($scope, members, timeZones, settings, bladeNavigationService) {
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

    $scope.pageSize = 50;
    $scope.organizations = [];

    $scope.fetch = function ($select, $event) {
        if (!$event) {
            // it's first call or the call from filter 
            $select.page = 0;
        } else {
            // This is a call from "Load more..." button
            $event.stopPropagation();
            $event.preventDefault();
            $select.page++;
        }

        members.search(
            {
                memberType: 'Organization',
                SearchPhrase: $select.search,
                deepSearch: true,
                take: $scope.pageSize,
                skip: $select.page * $scope.pageSize
            },
            function (data) {
                if (data.results.length < $scope.pageSize)
                    $select.loaded = true;
                else 
                    $select.loaded = false;

                if ($event)
                    $scope.organizations = $scope.organizations.concat(data.results);
                else 
                    $scope.organizations = data.results;
            });
    }

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
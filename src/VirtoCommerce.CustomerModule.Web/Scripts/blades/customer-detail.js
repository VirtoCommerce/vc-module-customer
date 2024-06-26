angular.module('virtoCommerce.customerModule').controller('virtoCommerce.customerModule.customerDetailController',
    ['$scope', 'platformWebApp.common.timeZones', 'platformWebApp.settings', 'platformWebApp.bladeNavigationService', 'virtoCommerce.coreModule.currency.currencyUtils',
        function ($scope, timeZones, settings, bladeNavigationService, currencyUtils) {
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
            blade.datepickers = {};
            blade.today = new Date();

            blade.open = function ($event, which) {
                $event.preventDefault();
                $event.stopPropagation();

                blade.datepickers[which] = true;
            };

            blade.timeZones = timeZones.query();
            blade.groups = settings.getValues({ id: 'Customer.MemberGroups' });
            blade.languages = settings.getValues({ id: 'VirtoCommerce.Core.General.Languages' });
            blade.currencies = currencyUtils.getCurrencies();

            blade.openGroupsDictionarySettingManagement = function () {
                var newBlade = {
                    id: 'settingDetailChild',
                    isApiSave: true,
                    currentEntityId: 'Customer.MemberGroups',
                    parentRefresh: function (data) { blade.groups = data; },
                    controller: 'platformWebApp.settingDictionaryController',
                    template: '$(Platform)/Scripts/app/settings/blades/setting-dictionary.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };
        }]);

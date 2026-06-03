angular.module('virtoCommerce.customerModule')
    .controller('virtoCommerce.customerModule.organizationMembershipsWidgetController',
        ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.customerModule.organizationMemberships',
        function ($scope, bladeNavigationService, organizationMemberships) {
            var blade = $scope.widget.blade;

            function refresh() {
                if (!blade.currentEntity || !blade.currentEntity.securityAccounts || !blade.currentEntity.securityAccounts.length) {
                    $scope.count = 0;
                    return;
                }

                var userId = blade.currentEntity.securityAccounts[0].id;
                organizationMemberships.count({ userId: userId }, function (data) {
                    $scope.count = data.count || 0;
                });
            }

            $scope.openBlade = function () {
                if (!blade.currentEntity || !blade.currentEntity.securityAccounts || !blade.currentEntity.securityAccounts.length) {
                    return;
                }
                var userId = blade.currentEntity.securityAccounts[0].id;
                var newBlade = {
                    id: 'organizationMembershipsList',
                    userId: userId,
                    contact: blade.currentEntity,
                    title: blade.title,
                    subtitle: 'customer.widgets.organization-memberships.blade-subtitle',
                    controller: 'virtoCommerce.customerModule.organizationMembershipsListController',
                    template: 'Modules/$(VirtoCommerce.Customer)/Scripts/blades/organization-memberships-list.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };

            $scope.$watch('widget.blade.currentEntity', refresh);
        }]);

angular.module('virtoCommerce.customerModule')
    .controller('virtoCommerce.customerModule.accountContactWidgetController', ['$scope',
        'platformWebApp.bladeNavigationService', 'virtoCommerce.customerModule.members',
        'virtoCommerce.customerModule.memberTypesResolverService',
        'platformWebApp.dialogService',
        function ($scope, bladeNavigationService, members, memberTypesResolverService, dialogService) {

        var blade = $scope.widget.blade;

        $scope.currentEntity = blade.currentEntity || blade.data;
        $scope.currentMember = null;

        if ($scope.currentEntity.memberId) {
            members.get({ id: $scope.currentEntity.memberId }, function (member) {
                $scope.currentMember = member;
            });
        }

        $scope.openBlade = function () {
            if (!$scope.currentMember) {
                return;
            }
            var foundTemplate = memberTypesResolverService.resolve($scope.currentMember.memberType);
            if (foundTemplate) {
                var newBlade = angular.copy(foundTemplate.detailBlade);
                newBlade.currentEntity = $scope.currentMember;
                newBlade.currentEntityId = $scope.currentMember.id;
                newBlade.isNew = false;
                newBlade.fromAccount = true;
                bladeNavigationService.showBlade(newBlade, blade);
            } else {
                dialogService.showNotificationDialog({
                    id: "error",
                    title: "customer.dialogs.unknown-member-type.title",
                    message: "customer.dialogs.unknown-member-type.message",
                    messageValues: { memberType: $scope.currentMember.memberType },
                });
            }
        };

    }]);

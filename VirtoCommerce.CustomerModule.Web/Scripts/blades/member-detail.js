angular.module('virtoCommerce.customerModule')
.controller('virtoCommerce.customerModule.memberDetailController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.customerModule.members', 'platformWebApp.dynamicProperties.api', function ($scope, bladeNavigationService, members, dynamicPropertiesApi) {
    var blade = $scope.blade;
    blade.updatePermission = 'customer:update';

    blade.refresh = function (parentRefresh) {
        if (blade.isNew) {
            blade.currentEntity = angular.extend({
                dynamicProperties: [],
                addresses: [],
                phones: [],
                emails: []
            }, blade.currentEntity);
        } else {
            blade.isLoading = true;

            members.get({ id: blade.currentEntity.id }, initializeBlade);

            if (parentRefresh) {
                blade.parentBlade.refresh(true);
            }
        }
    };

    blade.fillDynamicProperties = function () {
        dynamicPropertiesApi.query({ id: blade.memberTypeDefinition.fullTypeName }, function (results) {
            _.each(results, function (x) {
                x.displayNames = undefined;
                x.values = [];
            });
            blade.currentEntity.dynamicProperties = results;
            initializeBlade(blade.currentEntity);
        });
    };

    function initializeBlade(data) {
        blade.currentEntity = angular.copy(data);
        blade.origEntity = data;
        blade.isLoading = false;
    }

    function isDirty() {
        return !angular.equals(blade.currentEntity, blade.origEntity) && !blade.isNew && blade.hasUpdatePermission();
    }

    function canSave() {
        return isDirty() && $scope.formScope && $scope.formScope.$valid;
    }

    $scope.saveChanges = function () {
        blade.isLoading = true;

        if (blade.isNew) {
            members.save(blade.currentEntity,
                function () {
                    blade.parentBlade.refresh(true);
                    blade.origEntity = blade.currentEntity;
                    $scope.bladeClose();
                });
        } else {
            members.update(blade.currentEntity,
                function () { blade.refresh(true); });
        }
    };

    $scope.setForm = function (form) {
        $scope.formScope = form;
    }

    blade.onClose = function (closeCallback) {
        bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, $scope.saveChanges, closeCallback, "customer.dialogs.member-save.title", "customer.dialogs.member-save.message");
    };

    if (!blade.isNew) {
        blade.toolbarCommands = [
        {
            name: "platform.commands.save",
            icon: 'fa fa-save',
            executeMethod: $scope.saveChanges,
            canExecuteMethod: canSave,
            permission: blade.updatePermission
        },
        {
            name: "platform.commands.reset",
            icon: 'fa fa-undo',
            executeMethod: function () {
                angular.copy(blade.origEntity, blade.currentEntity);
            },
            canExecuteMethod: isDirty,
            permission: blade.updatePermission
        }
        ];
    }

    blade.headIcon = blade.memberTypeDefinition.icon;

    blade.refresh(false);
}]);
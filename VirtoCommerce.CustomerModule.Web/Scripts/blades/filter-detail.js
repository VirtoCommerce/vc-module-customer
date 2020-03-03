angular.module('virtoCommerce.customerModule')
    .controller('virtoCommerce.customerModule.filterDetailController', ['$scope', '$localStorage', '$translate',
        function ($scope, $localStorage, $translate) {
            var blade = $scope.blade;

            $scope.saveChanges = function () {
                blade.currentEntity.lastUpdateTime = new Date().getTime();
                angular.copy(blade.currentEntity, blade.origEntity);
                if (blade.isNew) {
                    $localStorage.customerSearchFilters.splice(0, 0, blade.origEntity);
                    $localStorage.customerSearchFilterId = blade.origEntity.id;
                    blade.parentBlade.filter.current = blade.origEntity;
                    blade.isNew = false;
                }

                initializeBlade(blade.origEntity);
                blade.parentBlade.filter.change(true);
            };

            function initializeBlade(data) {
                blade.currentEntity = angular.copy(data);
                blade.origEntity = data;
                blade.isLoading = false;

                blade.title = blade.isNew ? 'customer.blades.filter-detail.new-title' : data.name;
                blade.subtitle = blade.isNew ? 'customer.blades.filter-detail.new-subtitle' : 'customer.blades.filter-detail.subtitle';
            }

            var formScope;
            $scope.setForm = function (form) { formScope = form; };

            function isDirty() {
                return !angular.equals(blade.currentEntity, blade.origEntity);
            }

            blade.headIcon = 'fa-filter';

            blade.toolbarCommands = [
                {
                    name: "core.commands.apply-filter", icon: 'fa fa-filter',
                    executeMethod: function () {
                        $scope.saveChanges();
                    },
                    canExecuteMethod: function () {
                        return formScope && formScope.$valid;
                    }
                },
                {
                    name: "platform.commands.reset", icon: 'fa fa-undo',
                    executeMethod: function () {
                        angular.copy(blade.origEntity, blade.currentEntity);
                    },
                    canExecuteMethod: isDirty
                },
                {
                    name: "platform.commands.delete", icon: 'fa fa-trash-o',
                    executeMethod: deleteEntry,
                    canExecuteMethod: function () {
                        return !blade.isNew;
                    }
                }];


            function deleteEntry() {
                blade.parentBlade.filter.current = null;
                $localStorage.customerSearchFilters.splice($localStorage.customerSearchFilters.indexOf(blade.origEntity), 1);
                delete $localStorage.customerSearchFilterId;
                blade.parentBlade.filter.change();
            }

            // actions on load        
            if (blade.isNew) {
                $translate('customer.blades.member-list.labels.unnamed-filter').then(function (result) {
                    initializeBlade({ id: new Date().getTime(), name: result });
                });
            } else {
                initializeBlade(blade.data);
            }
        }]);

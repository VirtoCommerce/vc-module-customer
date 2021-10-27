angular.module('virtoCommerce.customerModule.common')
    .controller('virtoCommerce.customerModule.common.coreAddressDetailController', ['$scope', '$filter', 'platformWebApp.common.countries', 'platformWebApp.dialogService', 'platformWebApp.metaFormsService', 'platformWebApp.bladeNavigationService', function ($scope, $filter, countries, dialogService, metaFormsService, bladeNavigationService) {
        var blade = $scope.blade;

        blade.addressTypes = ['Billing', 'Shipping', 'BillingAndShipping'];
        blade.metaFields = blade.metaFields && blade.metaFields.length ? blade.metaFields : metaFormsService.getMetaFields('CustomeraddressDetails');
        if (blade.currentEntity.isNew) {
            blade.currentEntity.addressType = blade.addressTypes[1];
        }
        blade.origEntity = blade.currentEntity;
        blade.currentEntity = angular.copy(blade.origEntity);
        blade.countries = countries.query();


        blade.defaultAddress = blade.currentEntity.isDefault;


        blade.countries.$promise.then(
            (allCountries) => {
                $scope.$watch('blade.currentEntity.countryCode', (countryCode, old) => {
                    if (countryCode) {
                        var country = _.findWhere(allCountries, { id: countryCode });
                        if (country) {
                            blade.currentEntity.countryName = country.name;

                            if (countryCode !== old) {
                                blade.currentEntity.regionName = undefined;
                                currentRegions = [];
                            }

                            if (country.regions) {
                                currentRegions = country.regions;
                            } else {
                                countries.queryRegions(countryCode).$promise.then((regions) => {
                                    country.regions = regions;
                                    currentRegions.push(...regions);
                                });
                            }
                        }
                    }
                });
            }
        );

        var currentRegions = [];
        if (blade.currentEntity.regionName && !blade.currentEntity.regionId) {
            addRegion(currentRegions, blade.currentEntity.regionName);
        }

        blade.getRegions = function (search) {
            var results = currentRegions;
            if (search && search.length > 1) {
                var filtered = $filter('filter')(results, search);
                if (!_.some(filtered)) {
                    addRegion(results, search);
                } else if (filtered.length > 1) { // remove other (added) records
                    filtered = _.filter(filtered, (x) => !x.id && x.displayName.length > search.length);
                    for (var x of filtered) {
                        results.splice(_.indexOf(results, x), 1);
                    }
                }
            }

            return results;
        };

        function addRegion(regions, name) {
            regions.unshift({ name: name, displayName: name });
        }

        $scope.$watch('blade.currentEntity.regionName', function (regionName, old) {
            if (regionName === old) return;

            var newId = null;
            if (regionName) {
                var region = _.findWhere(currentRegions, { name: regionName });
                if (region) {
                    newId = region.id;
                }
            }

            blade.currentEntity.regionId = newId;
        });

        blade.toolbarCommands = [
            {
                name: "platform.commands.save", icon: 'fas fa-save',
                executeMethod: function () {

                    // Assigning a default address
                    if (blade.currentEntity.isNew) { // If you added a new address
                        if (blade.numberOfAddresses(blade.currentEntity.addressType) == 0) { // If he will the only one
                            blade.currentEntity.isDefault = true;
                        }
                        else {
                            if (blade.currentEntity.isDefault === true) { // If at creation made the default address
                                blade.searchDefaultAddress(blade.currentEntity.addressType);
                            }
                            else {
                                blade.currentEntity.isDefault = false;
                            }
                        }
                    }
                    else {
                        if (!angular.equals(blade.currentEntity.addressType, blade.origEntity.addressType)) { // If the type of the current address has changed
                            blade.currentEntity.isDefault = false;
                            if (blade.numberOfAddresses(blade.currentEntity.addressType) == 0) { // The current address will be the only one in the group
                                blade.currentEntity.isDefault = true;
                            }
                            if (blade.numberOfAddresses(blade.origEntity.addressType) == 2 && blade.origEntity.addressType !== blade.addressTypes[2]) {
                                blade.searchSecondAddress(blade.origEntity.addressType, blade.currentEntity.name);
                            }
                        }
                        else {
                            blade.searchDefaultAddress(blade.currentEntity.addressType);
                        }
                    }
                    if (blade.currentEntity.addressType == blade.addressTypes[2]) { // If the current address is from BillingAndShipping
                        blade.currentEntity.isDefault = false;
                    }

                    if (blade.confirmChangesFn) {
                        blade.confirmChangesFn(blade.currentEntity);
                    }
                    angular.copy(blade.currentEntity, blade.origEntity);
                    $scope.bladeClose();
                },
                canExecuteMethod: canSave,
                permission: blade.updatePermission
            },
            {
                name: "platform.commands.reset", icon: 'fa fa-undo',
                executeMethod: function () {
                    angular.copy(blade.origEntity, blade.currentEntity);
                },
                canExecuteMethod: isDirty
            },
            {
                name: "platform.commands.delete", icon: 'fas fa-trash-alt',
                executeMethod: deleteEntry,
                canExecuteMethod: function () {
                    return !blade.currentEntity.isNew;
                }
            },
            {
                name: "core.commands.default", icon: 'fas fa-flag',
                executeMethod: function () {
                    blade.currentEntity.isDefault = true;
                },
                canExecuteMethod: function () {
                    if (blade.currentEntity.addressType == 'BillingAndShipping')
                        return false;
                    else
                        return !blade.currentEntity.isDefault;
                },
                meta: "default"
            }
        ];

        blade.isLoading = false;

        blade.onClose = function (closeCallback) {
            bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, $scope.saveChanges, closeCallback, "core.dialogs.address-save.title", "core.dialogs.address-save.message");
        };

        $scope.setForm = function (form) {
            $scope.formScope = form;
        };

        function isDirty() {
            return !angular.equals(blade.currentEntity, blade.origEntity);
        }

        function canSave() {
            return isDirty() && $scope.formScope && $scope.formScope.$valid;
        }

        function deleteEntry() {
            var dialog = {
                id: "confirmDelete",
                title: "core.dialogs.address-delete.title",
                message: "core.dialogs.address-delete.message",
                callback: function (remove) {
                    if (remove) {
                        if (blade.deleteFn) {
                            blade.deleteFn(blade.currentEntity);
                        }
                        $scope.bladeClose();
                        if (blade.numberOfAddresses(blade.origEntity.addressType) == 1 && blade.currentEntity.addressType !== blade.addressTypes[2]) {
                            blade.searchSecondAddress(blade.origEntity.addressType, blade.currentEntity.name);
                        }
                    }
                }
            }
            dialogService.showConfirmationDialog(dialog);
        }
    }]);

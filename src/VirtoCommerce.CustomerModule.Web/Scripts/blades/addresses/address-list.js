angular.module('virtoCommerce.customerModule')
    .controller('virtoCommerce.customerModule.addressListController',
        ['$timeout', '$scope', 'platformWebApp.bladeNavigationService',
            function ($timeout, $scope, bladeNavigationService) {
                const blade = $scope.blade;
                $scope.selectedItem = null;

                $scope.openDetailBlade = function (address) {
                    if (!address) {
                        address = { isNew: true };
                    }

                    $scope.selectedItem = address;
                    const newBlade = {
                        id: 'customerAddressDetails',
                        currentEntity: address,
                        title: blade.title,
                        controller: 'virtoCommerce.customerModule.addressDetailsController',
                        confirmChangesFn: function (changedAddress) {
                            changedAddress.name = $scope.getAddressName(changedAddress);
                            if (changedAddress.isNew) {
                                changedAddress.isNew = undefined;
                                blade.currentEntities.push(changedAddress);
                                if (blade.confirmChangesFn) {
                                    blade.confirmChangesFn(changedAddress);
                                }
                            }
                        },
                        deleteFn: function (deletedAddress) {
                            const toRemove = _.find(blade.currentEntities, function (x) {
                                return angular.equals(x, deletedAddress)
                            });

                            if (toRemove) {
                                const idx = blade.currentEntities.indexOf(toRemove);
                                blade.currentEntities.splice(idx, 1);

                                if (blade.deleteFn) {
                                    blade.deleteFn(deletedAddress);
                                }
                            }
                        },
                        numberOfAddresses: function (addressType) {
                            let count = 0;
                            blade.currentEntities.find((defAddress) => {
                                if (defAddress.addressType === addressType) {
                                    count++;
                                }
                            });
                            return count;
                        },
                        searchSecondAddress: function (addressType, name) {
                            blade.currentEntities.find((defAddress, i) => {
                                if (defAddress.addressType === addressType && defAddress.name !== name) {
                                    blade.currentEntities[i].isDefault = true;
                                }
                            });
                        },
                        searchDefaultAddress: function (addressType) {
                            blade.currentEntities.find((defAddress, i) => {
                                if (defAddress.addressType === addressType && defAddress.isDefault) {
                                    blade.currentEntities[i].isDefault = false;
                                }
                            });
                        },

                        template: 'Modules/$(VirtoCommerce.Customer)/Scripts/blades/addresses/address-details.tpl.html'
                    };
                    bladeNavigationService.showBlade(newBlade, $scope.blade);
                }

                $scope.getAddressName = function (address) {
                    return [address.countryCode, address.regionName, address.city, address.line1].join(", ");
                };

                blade.headIcon = blade.parentBlade.headIcon;

                blade.toolbarCommands = [
                    {
                        name: 'platform.commands.add', icon: 'fas fa-plus',
                        executeMethod: function () {
                            $scope.openDetailBlade();
                        },
                        canExecuteMethod: function () {
                            return true;
                        }
                    }
                ];

                blade.isLoading = false;

                // open blade for new setting
                if (!_.some(blade.currentEntities)) {
                    $timeout($scope.openDetailBlade, 60, false);
                }
            }
        ]);

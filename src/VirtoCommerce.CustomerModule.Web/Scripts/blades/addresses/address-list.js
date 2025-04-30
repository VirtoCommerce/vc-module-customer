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
                        id: 'coreAddressDetail',
                        currentEntity: address,
                        title: blade.title,
                        controller: 'virtoCommerce.customerModule.addressDetailsController',
                        confirmChangesFn: function (address) {
                            address.name = $scope.getAddressName(address);
                            if (address.isNew) {
                                address.isNew = undefined;
                                blade.currentEntities.push(address);
                                if (blade.confirmChangesFn) {
                                    blade.confirmChangesFn(address);
                                }
                            }
                        },
                        deleteFn: function (address) {
                            const toRemove = _.find(blade.currentEntities, function (x) { return angular.equals(x, address) });

                            if (toRemove) {
                                const idx = blade.currentEntities.indexOf(toRemove);
                                blade.currentEntities.splice(idx, 1);

                                if (blade.deleteFn) {
                                    blade.deleteFn(address);
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

angular.module('virtoCommerce.customerModule.common')
    .controller('virtoCommerce.customerModule.common.coreAddressListController', ['$timeout', '$scope', 'platformWebApp.bladeNavigationService', function ($timeout, $scope, bladeNavigationService) {
        var blade = $scope.blade;
        $scope.selectedItem = null;

        $scope.openDetailBlade = function (address) {
            if (!address) {
                address = { isNew: true };
            }
            $scope.selectedItem = address;

            var newBlade = {
                id: 'coreAddressDetail',
                currentEntity: address,
                title: blade.title,
                subtitle: 'core.blades.address-detail.subtitle',
                controller: 'virtoCommerce.customerModule.common.coreAddressDetailController',
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
                    var toRemove = _.find(blade.currentEntities, function (x) { return angular.equals(x, address) });
                    if (toRemove) {
                        var idx = blade.currentEntities.indexOf(toRemove);
                        blade.currentEntities.splice(idx, 1);
                        if (blade.deleteFn) {
                            blade.deleteFn(address);
                        }
                    }
                },
                numberOfAddresses: function (addressType) {
                    var count = 0;
                    blade.currentEntities.find((defAddress) => {
                        if (defAddress.addressType === addressType) {
                            count++;
                        }
                    });
                    return count;
                },
                searchDefaultAddress: function (addressType) {
                    blade.currentEntities.find((defAddress, i) => {
                        if (defAddress.addressType === addressType && defAddress.isDefault) {
                            blade.currentEntities[i].isDefault = false;
                        }
                    });
                },

                template: 'Modules/$(VirtoCommerce.Customer)/Scripts/common/blades/address-detail.tpl.html'
            };
            bladeNavigationService.showBlade(newBlade, $scope.blade);
        }

        $scope.getAddressName = function (address) {
            return [address.countryCode, address.regionName, address.city, address.line1].join(", ");
        };

        blade.headIcon = blade.parentBlade.headIcon;

        blade.toolbarCommands = [
            {
                name: "platform.commands.add", icon: 'fas fa-plus',
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
    }]);

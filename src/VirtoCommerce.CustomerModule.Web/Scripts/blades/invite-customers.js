angular.module('virtoCommerce.customerModule').controller('virtoCommerce.customerModule.inviteCustomersController', [
    '$scope', '$timeout',
    'platformWebApp.bladeNavigationService', 'platformWebApp.metaFormsService',
    'virtoCommerce.customerModule.organizations', 'virtoCommerce.storeModule.stores', 'virtoCommerce.customerModule.customers',
    function ($scope, $timeout, bladeNavigationService, metaFormsService, organizations, stores, customers) {
        var blade = $scope.blade;
        blade.title = 'customer.blades.invite-customers.title';

        blade.languagesReloaded = true;
        blade.storeLanguages = [];

        blade.metaFields = blade.metaFields ? blade.metaFields : metaFormsService.getMetaFields('inviteCustomerDetails');

        function initializeBlade() {
            blade.currentEntity = {
                message: null,
                emails: [],
                roleIds: []
            };

            if (blade.selectedStore) {
                blade.currentEntity.storeId = blade.selectedStore.id;

                blade.selectStore(blade.selectedStore);
            }

            if (blade.selectedOrganization) {
                blade.currentEntity.organizationId = blade.selectedOrganization.id;
            }

            blade.isLoading = false;
        }

        blade.fetchStores = function (criteria) {
            return stores.search(criteria)
        }

        blade.fetchStoreLanguages = function () {
            return blade.storeLanguages;
        }

        blade.fetchOrganizations = function (criteria) {
            criteria.deepSearch = true;
            return organizations.search(criteria);
        }

        blade.fetchRoles = function () {
            let promise = customers.availableRoles().$promise.then(function (response) {
                var results = response.results;

                if (results.length && !blade.currentEntity.roleId) {
                    blade.currentEntity.roleId = results[0].id;
                }

                var result = {
                    totalCount: response.totalCount,
                    results: results
                };

                return result;
            });

            return {
                $promise: promise
            };
        }

        blade.selectStore = function (item) {
            blade.storeLanguages = [];

            angular.forEach(item.languages, function (language) {
                blade.storeLanguages.push({ id: language, name: language });
            });

            if (item.defaultLanguage) {
                blade.currentEntity.cultureName = _.findWhere(blade.storeLanguages, { id: item.defaultLanguage });
            }

            reloadLanguages();
        }

        function reloadLanguages() {
            blade.languagesReloaded = false;

            $timeout(function () {
                blade.languagesReloaded = true;
            }, 0);
        }

        $scope.setForm = function (form) {
            $scope.formScope = form;
        }

        function canSave() {
            return $scope.formScope && $scope.formScope.$valid;
        }

        blade.toolbarCommands = [
            {
                name: "customer.blades.invite-customers.commands.send-invitations",
                icon: 'fas fa-paper-plane',
                executeMethod: function () {
                    blade.isLoading = true;

                    if (blade.currentEntity.emailsString) {
                        blade.currentEntity.emails = blade.currentEntity.emailsString.split(/[\s,;]+/)
                            .map(e => e.trim())
                            .filter(e => e.length);
                    }

                    if (blade.currentEntity.roleId) {
                        blade.currentEntity.roleIds = [blade.currentEntity.roleId];
                    }

                    customers.invite(blade.currentEntity,
                        function (data) {
                            blade.isLoading = false;
                            $scope.bladeClose();
                        },
                        function (error) {
                            let errorData = error;
                            if (error.status === 400) {
                                errorData = {
                                    status: error.status,
                                    statusText: error.statusText,
                                    data: {
                                        errors: error.data.errors.map(err => err.description)
                                    }
                                };
                            }

                            bladeNavigationService.setError(errorData, blade);
                        })
                },
                canExecuteMethod: canSave
            }
        ];

        $scope.sendInvites = function () {
            blade.isLoading = true;


        }

        initializeBlade();
    }]);

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
                emails: []
            };

            if (blade.selectStore) {
                blade.storeId = blade.selectStore.id;

                reloadLanguages();
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
            return customers.availableRoles();
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
                name: "platform.commands.save",
                icon: 'fas fa-save',
                executeMethod: function () {
                    blade.isLoading = true;

                    if (blade.currentEntity.emailsString) {
                        blade.currentEntity.emails = blade.currentEntity.emailsString.split(/[\s,;]+/)
                            .map(e => e.trim())
                            .filter(e => e.length);
                    }

                    customers.invite(blade.currentEntity,
                        function (data) {
                            blade.isLoading = false;
                            $scope.bladeClose();
                        },
                        function (error) {
                            bladeNavigationService.setError('Error ' + error.status, blade);
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

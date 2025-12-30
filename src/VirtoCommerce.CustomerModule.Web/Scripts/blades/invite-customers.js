angular.module('virtoCommerce.customerModule').controller('virtoCommerce.customerModule.inviteCustomersController', [
    '$scope', '$timeout', '$translate',
    'platformWebApp.bladeNavigationService', 'platformWebApp.metaFormsService', 'platformWebApp.dialogService',
    'virtoCommerce.customerModule.organizations', 'virtoCommerce.storeModule.stores', 'virtoCommerce.customerModule.customers',
    function ($scope, $timeout, $translate, bladeNavigationService, metaFormsService, dialogService, organizations, stores, customers) {
        var blade = $scope.blade;
        blade.title = 'customer.blades.invite-customers.title';

        blade.languagesReloaded = true;
        blade.storeLanguages = [];

        blade.metaFields = blade.metaFields ? blade.metaFields : metaFormsService.getMetaFields('inviteCustomerDetails');

        function initializeBlade() {
            blade.currentEntity = {
                message: null,
                emails: [],
                emailObjects: [],
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
            const promise = customers.availableRoles().$promise.then(function (response) {
                var results = response.results;

                if (results.length && !blade.currentEntity.roleId) {
                    blade.currentEntity.roleId = results[0].id;
                }

                return {
                    totalCount: response.totalCount,
                    results: results
                };
            });

            return {
                $promise: promise
            };
        }

        blade.emailAdding = function (tag) {

            if (tag.text) {
                // collect all possible emails from the text
                const possibleEmails = tag.text.match(/\b[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}\b/g);

                if (possibleEmails && possibleEmails.length) {
                    // create a sting of emails separated by semicolon and pass to the tag text
                    tag.text = possibleEmails.join(';');
                }

                // this will trigger the separation by semicolon and add multiple tags to the tag-input
                return true;
            }

            return false;
        }

        blade.emailAdded = function (tag) {
            // tags-input can delete the binding object, for some reason
            if (!blade.currentEntity.emailObjects) {
                blade.currentEntity.emailObjects = [];
            }

            // force remove the original emails if present
            const tagIndex = _.findIndex(blade.currentEntity.emailObjects, function (obj) {
                return obj.text === tag.text;
            })

            if (tagIndex >= 0) {
                blade.currentEntity.emailObjects.splice(tagIndex, 1);
            }

            // check if email is email by regexp
            const re = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;

            // tag.text is either a single email or multiple emails separated by semicolon
            // remove already existing emails in blade.currentEntity.emailObjects from the possible emails
            const emails = tag.text
                .split(';')
                .map(e => e.trim())
                .filter(e => e.length)
                .filter(e => re.test(e))
                .filter(e => !blade.currentEntity.emailObjects.some(eo => eo.text.toLowerCase() === e.toLowerCase()));

            if (emails.length) {
                emails.forEach(email => {
                    const emailIndex = _.findIndex(blade.currentEntity.emailObjects, function (obj) {
                        return obj.text === email;
                    })

                    if (emailIndex >= 0) {
                        blade.currentEntity.emailObjects.splice(emailIndex, 1);
                    }
                });

                // add multiple tags for each email
                emails.forEach(email => {
                    blade.currentEntity.emailObjects.push({ text: email });
                });
            }
        }

        blade.selectStore = function (item) {
            blade.storeLanguages = [];

            angular.forEach(item.languages, function (language) {
                blade.storeLanguages.push({ id: language, name: language });
            });

            if (item.defaultLanguage) {
                blade.currentEntity.language = _.findWhere(blade.storeLanguages, { id: item.defaultLanguage });
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

                    if (blade.currentEntity.emailObjects) {
                        blade.currentEntity.emails = blade.currentEntity.emailObjects
                            .map(e => e.text.trim())
                            .filter(e => e.length);
                    }

                    if (blade.currentEntity.roleId) {
                        blade.currentEntity.roleIds = [blade.currentEntity.roleId];
                    }

                    if (blade.currentEntity.language) {
                        blade.currentEntity.cultureName = blade.currentEntity.language.id;
                    }

                    blade.inviteEntity = angular.copy(blade.currentEntity);

                    // clear exces properties
                    delete blade.inviteEntity.emailObjects;
                    delete blade.inviteEntity.roleId;
                    delete blade.inviteEntity.language;

                    customers.invite(blade.inviteEntity,
                        function (data) {
                            blade.isLoading = false;

                            if (data.errors && data.errors.length) {
                                const errorData = {
                                    status: $translate.instant('customer.blades.invite-customers.title'),
                                    statusText: $translate.instant('customer.blades.invite-customers.labels.error'),
                                    data: {
                                        errors: []
                                    }
                                };

                                if (Array.isArray(data.errors)) {
                                    errorData.data.errors = data.errors.map(err => err.description);
                                }
                                bladeNavigationService.setError(errorData, blade);

                                var errorDialog = {
                                    id: 'errosInviteCustomers',
                                    title: 'customer.dialogs.invite-customers-error.title',
                                    message: 'customer.dialogs.invite-customers-error.message'
                                };
                                dialogService.showErrorDialog(errorDialog);
                            }
                            else {
                                var successDialog = {
                                    id: 'successInviteCustomers',
                                    title: 'customer.dialogs.invite-customers-success.title',
                                    message: 'customer.dialogs.invite-customers-success.message',
                                    callback: function () {
                                        blade.currentEntity.emailObjects = [];
                                        blade.currentEntity.emails = [];
                                    }
                                };
                                dialogService.showSuccessDialog(successDialog);
                            }
                        },
                        function (error) {
                            bladeNavigationService.setError(error, blade);
                        })
                },
                canExecuteMethod: canSave
            }
        ];

        initializeBlade();
    }]);

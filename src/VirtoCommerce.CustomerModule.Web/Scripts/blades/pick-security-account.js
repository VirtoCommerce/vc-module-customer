angular.module('virtoCommerce.customerModule')
    .controller('virtoCommerce.customerModule.pickSecurityAccountController',
        ['$scope', 'platformWebApp.accounts', 'platformWebApp.uiGridHelper',
            'platformWebApp.bladeNavigationService', 'platformWebApp.bladeUtils',
            'virtoCommerce.customerModule.members', 'platformWebApp.dialogService',
            'platformWebApp.settings', 'platformWebApp.roles', 'platformWebApp.clipboardService',
            function ($scope, accounts, uiGridHelper, bladeNavigationService, bladeUtils, members, dialogService, settings, roles, clipboardService) {
                var blade = $scope.blade;
                blade.headIcon = 'fas fa-key';
                blade.title = 'customer.blades.pick-security-account-list.title';
                blade.subtitle = 'customer.blades.pick-security-account-list.subtitle';

                $scope.uiGridConstants = uiGridHelper.uiGridConstants;

                // The blade reuses the platform account-list.tpl.html template, so the filter state below
                // must match what that template binds to (see platformWebApp.accountListController)
                var filter = $scope.filter = {
                    keyword: '',
                    onlyLocked: false,
                    emailNotConfirmed: false,
                    userType: '',
                    status: '',
                    role: '',
                    datePreset: '',
                    loginStartDate: null,
                    loginEndDate: null,

                    hasActiveFilters: function () {
                        return filter.onlyLocked ||
                            filter.emailNotConfirmed ||
                            filter.userType ||
                            filter.status ||
                            filter.role ||
                            filter.datePreset;
                    },

                    clearFilters: function () {
                        filter.onlyLocked = false;
                        filter.emailNotConfirmed = false;
                        filter.userType = '';
                        filter.status = '';
                        filter.role = '';
                        filter.datePreset = '';
                        filter.loginStartDate = null;
                        filter.loginEndDate = null;
                        filter.criteriaChanged();
                    },

                    criteriaChanged: function () {
                        computeDateRange();
                        if ($scope.pageSettings.currentPage > 1) {
                            $scope.pageSettings.currentPage = 1;
                        } else {
                            blade.refresh();
                        }
                    }
                };

                function computeDateRange() {
                    if (filter.datePreset === 'custom') {
                        return;
                    }
                    var now = new Date();
                    var startOfToday = new Date(now.getFullYear(), now.getMonth(), now.getDate());
                    filter.loginStartDate = null;
                    filter.loginEndDate = null;

                    switch (filter.datePreset) {
                        case 'today':
                            filter.loginStartDate = startOfToday;
                            break;
                        case 'yesterday':
                            filter.loginStartDate = new Date(startOfToday.getTime() - 86400000);
                            filter.loginEndDate = startOfToday;
                            break;
                        case 'last7':
                            filter.loginStartDate = new Date(startOfToday.getTime() - 7 * 86400000);
                            break;
                        case 'last30':
                            filter.loginStartDate = new Date(startOfToday.getTime() - 30 * 86400000);
                            break;
                        default:
                            break;
                    }
                }

                blade.searchText = '';
                $scope.$watch('blade.searchText', function (newVal, oldVal) {
                    if (newVal !== oldVal) {
                        filter.keyword = newVal;
                        filter.criteriaChanged();
                    }
                });

                blade.refresh = function () {
                    blade.isLoading = true;

                    var searchCriteria = {
                        keyword: filter.keyword,
                        sort: uiGridHelper.getSortExpression($scope),
                        skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                        take: $scope.pageSettings.itemsPerPageCount
                    };

                    if (filter.onlyLocked) {
                        searchCriteria.onlyLocked = true;
                    }
                    if (filter.emailNotConfirmed) {
                        searchCriteria.emailConfirmed = false;
                    }
                    if (filter.userType) {
                        searchCriteria.userType = filter.userType;
                    }
                    if (filter.status) {
                        searchCriteria.status = filter.status;
                    }
                    if (filter.role) {
                        searchCriteria.roles = [filter.role];
                    }
                    if (filter.loginStartDate) {
                        searchCriteria.loginStartDate = filter.loginStartDate;
                    }
                    if (filter.loginEndDate) {
                        searchCriteria.loginEndDate = filter.loginEndDate;
                    }

                    accounts.search(searchCriteria, function (data) {
                        blade.isLoading = false;

                        $scope.pageSettings.totalItems = data.totalCount;
                        blade.currentEntities = data.results;
                    }, function (error) {
                        bladeNavigationService.setError('Error ' + error.status, blade);
                    });
                };

                blade.selectNode = function (node) {
                    $scope.selectedNodeId = node.id;

                    var newBlade = {
                        id: 'listItemChild',
                        controller: 'platformWebApp.accountDetailController',
                        template: '$(Platform)/Scripts/app/security/blades/account-detail.tpl.html',
                        data: node,
                        title: node.userName,
                        subtitle: blade.subtitle,
                    };

                    bladeNavigationService.showBlade(newBlade, blade);
                };

                function isRowSelectable(row) {
                    return row.entity.memberId !== blade.currentEntity.memberId;
                }

                function rowSelectionChanged(row) {
                    if (row.isSelected && row.entity.memberId) {
                        members.get({ id: row.entity.memberId }, function (member) {
                            if (member) {
                                const dialog = {
                                    id: 'confirmLinkAccount',
                                    title: 'customer.dialogs.confirm-account-link.title',
                                    message: 'customer.dialogs.confirm-account-link.message',
                                    messageValues: { memberName: member.name },
                                    callback: function (isConfirmed) {
                                        row.isSelected = isConfirmed;
                                    },
                                    callbackOnDismiss: function (_) {
                                        row.isSelected = false;
                                    },
                                };
                                dialogService.showConfirmationDialog(dialog);
                            }
                        });
                    }
                }

                function linkAccounts(selectedAccounts) {
                    blade.isLoading = true;

                    const updatePromises = selectedAccounts.map(account => {
                        return new Promise((resolve, reject) => {
                            account.memberId = blade.currentEntity.memberId;
                            accounts.update(account, function (result) {
                                if (result.succeeded) {
                                    resolve();
                                } else {
                                    bladeNavigationService.setError(result.errors.join(), blade);
                                    reject(result.errors);
                                }
                            });
                        });
                    });

                    Promise.all(updatePromises)
                        .then(() => {
                            blade.parentBlade.refresh();
                            $scope.bladeClose();
                        })
                        .finally(() => {
                            blade.isLoading = false;
                        });
                }

                blade.toolbarCommands = [
                    {
                        name: "platform.commands.save", icon: 'fas fa-save',
                        executeMethod: function () { linkAccounts($scope.gridApi.selection.getSelectedRows()); },
                        canExecuteMethod: () => $scope.gridApi?.selection?.getSelectedRows()?.length > 0
                    },
                    {
                        name: "platform.commands.refresh", icon: 'fa fa-refresh',
                        executeMethod: blade.refresh,
                        canExecuteMethod: function () {
                            return true;
                        }
                    }
                ];

                // filter options
                blade.accountTypes = [];
                blade.accountStatuses = [];
                blade.roles = [];

                settings.get({ id: 'VirtoCommerce.Platform.Security.AccountTypes' }, function (setting) {
                    blade.accountTypes = setting.allowedValues || [];
                });
                settings.get({ id: 'VirtoCommerce.Other.AccountStatuses' }, function (setting) {
                    blade.accountStatuses = setting.allowedValues || [];
                });
                roles.search({ take: 1000 }, function (data) {
                    blade.roles = data.results || [];
                }, function (error) {
                    console.error('Failed to load roles:', error);
                });

                blade.datePresets = [
                    { label: 'platform.blades.account-list.filter.date-any', value: '' },
                    { label: 'platform.blades.account-list.filter.date-today', value: 'today' },
                    { label: 'platform.blades.account-list.filter.date-yesterday', value: 'yesterday' },
                    { label: 'platform.blades.account-list.filter.date-last7', value: 'last7' },
                    { label: 'platform.blades.account-list.filter.date-last30', value: 'last30' },
                    { label: 'platform.blades.account-list.filter.date-custom', value: 'custom' }
                ];

                $scope.copy = function (text) {
                    clipboardService.copyText(text);
                };

                // ui-grid
                $scope.setGridOptions = function (gridOptions) {
                    gridOptions.enableSelectAll = false;
                    gridOptions.isRowSelectable = isRowSelectable;

                    uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                        uiGridHelper.bindRefreshOnSortChanged($scope);
                        gridApi.selection.on.rowSelectionChanged($scope, rowSelectionChanged);
                    });

                    bladeUtils.initializePagination($scope);
                };
            }]);

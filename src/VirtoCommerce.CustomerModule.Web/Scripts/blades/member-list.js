angular.module('virtoCommerce.customerModule')
    .controller('virtoCommerce.customerModule.memberListController', ['$scope', '$location', 'virtoCommerce.customerModule.members', 'platformWebApp.dialogService', 'platformWebApp.bladeUtils', 'platformWebApp.uiGridHelper', 'virtoCommerce.customerModule.memberTypesResolverService', 'platformWebApp.ui-grid.extension',
        function ($scope, $location, members, dialogService, bladeUtils, uiGridHelper, memberTypesResolverService, gridOptionExtension) {
            $scope.uiGridConstants = uiGridHelper.uiGridConstants;

            var blade = $scope.blade;
            blade.title = 'customer.blades.member-list.title';
            var bladeNavigationService = bladeUtils.bladeNavigationService;

            blade.refresh = function (parentRefresh) {
                blade.isLoading = true;
                var searchCriteria = getSearchCriteria();

                if (blade.searchCriteria) {
                    angular.extend(searchCriteria, blade.searchCriteria);
                }

                members.search(searchCriteria,
                    function (data) {
                        blade.isLoading = false;
                        $scope.pageSettings.totalItems = data.totalCount;
                        // precalculate icon
                        var memberTypeDefinition;
                        _.each(data.results, function (x) {
                            if (memberTypeDefinition = memberTypesResolverService.resolve(x.memberType)) {
                                x._memberTypeIcon = memberTypeDefinition.icon;
                            }
                        });
                        // add outerId property to the first element(if it doesn't exist) so that the outer id column appears
                        if (Array.isArray(data.results) && data.results.length && !data.results[0].outerId) {
                            data.results[0].outerId = null;
                        }
                        $scope.listEntries = data.results ? data.results : [];

                        //Set navigation breadcrumbs
                        setBreadcrumbs();
                    });

                if (parentRefresh && blade.parentRefresh) {
                    blade.parentRefresh();
                }
            };

            //Breadcrumbs
            function setBreadcrumbs() {
                if (blade.breadcrumbs) {
                    //Clone array (angular.copy leaves the same reference)
                    var breadcrumbs = blade.breadcrumbs.slice(0);

                    //prevent duplicate items
                    if (_.all(breadcrumbs, function (x) { return x.id !== blade.currentEntity.id; })) {
                        if (blade.currentEntity.id) {
                            var breadCrumb = generateBreadcrumb(blade.currentEntity.id, blade.currentEntity.name);
                            breadcrumbs.push(breadCrumb);
                        }
                    }
                    blade.breadcrumbs = breadcrumbs;
                } else {
                    blade.breadcrumbs = [];
                }
            }

            function generateBreadcrumb(id, name) {
                return {
                    id: id,
                    name: name,
                    blade: blade,
                    navigate: function () {
                        bladeNavigationService.closeBlade(blade,
                            function () {
                                blade.disableOpenAnimation = true;
                                bladeNavigationService.showBlade(blade, blade.parentBlade);
                            });
                    }
                };
            }

            blade.showDetailBlade = function (listItem, isNew) {
                blade.setSelectedNode(listItem);

                var foundTemplate = memberTypesResolverService.resolve(listItem.memberType);
                if (foundTemplate) {
                    var newBlade = angular.copy(foundTemplate.detailBlade);
                    newBlade.currentEntity = listItem;
                    newBlade.currentEntityId = listItem.id;
                    newBlade.isNew = isNew;
                    bladeNavigationService.showBlade(newBlade, blade);
                } else {
                    dialogService.showNotificationDialog({
                        id: "error",
                        title: "customer.dialogs.unknown-member-type.title",
                        message: "customer.dialogs.unknown-member-type.message",
                        messageValues: { memberType: listItem.memberType },
                    });
                }
            };

            $scope.delete = function (data) {
                deleteList([data]);
            };

            function getDeleteItemsInfo(data) {
                const map = new Map();

                for (const { memberType } of data) {
                    map.set(memberType, (map.get(memberType) ?? 0) + 1);
                }

                return Array.from(map, ([memberType, count]) => ({
                    memberType,
                    count
                }));
            }

            function deleteList(data) {
                const deleteItemsInfo = getDeleteItemsInfo(data);
                var dialog = {
                    id: "confirmDeleteItem",
                    title: "customer.dialogs.members-delete.title",
                    items: deleteItemsInfo.map(function (item) {
                        return { key: 'customer.dialogs.members-delete.' + item.memberType, count: item.count };
                    }),
                    callback: function (remove) {
                        if (remove) {
                            bladeNavigationService.closeChildrenBlades(blade, function () {
                                blade.isLoading = true;
                                var memberIds = _.pluck(data, 'id');

                                if (($scope.gridApi != undefined) && $scope.gridApi.selection.getSelectAllState()) {
                                    var searchCriteria = getSearchCriteria();
                                    members.delete(searchCriteria, function () {
                                        $scope.gridApi.selection.clearSelectedRows();
                                        blade.refresh(true);
                                    }
                                    );
                                }
                                else if (_.any(memberIds)) {
                                    members.remove({ ids: memberIds },
                                        function () { blade.refresh(true); });
                                }
                            });
                        }
                    }
                };
                dialogService.showDeleteConfirmationDialog(dialog);
            }

            blade.setSelectedNode = function (listItem) {
                $scope.selectedNodeId = listItem.id;

                $location.search({ memberId: listItem.id });
            };

            $scope.selectNode = function (listItem) {
                blade.setSelectedNode(listItem);

                var foundTemplate = memberTypesResolverService.resolve(listItem.memberType);
                if (foundTemplate && foundTemplate.knownChildrenTypes && foundTemplate.knownChildrenTypes.length) {
                    var newBlade = {
                        id: 'members' + (blade.level + 1),
                        level: blade.level + 1,
                        breadcrumbs: blade.breadcrumbs,
                        subtitle: 'customer.blades.member-list.subtitle',
                        subtitleValues: { name: listItem.name },
                        currentEntity: listItem,
                        controller: blade.controller,
                        template: blade.template,
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                } else {
                    blade.showDetailBlade(listItem);
                }
            };

            blade.headIcon = 'fa fa-user __customers';

            blade.toolbarCommands = [
                {
                    name: "platform.commands.refresh", icon: 'fa fa-refresh',
                    executeMethod: blade.refresh,
                    canExecuteMethod: function () {
                        return true;
                    }
                },
                {
                    name: "platform.commands.add", icon: 'fas fa-plus',
                    executeMethod: function () {
                        var newBlade = {
                            id: 'listItemChild',
                            currentEntity: blade.currentEntity,
                            title: 'customer.blades.member-add.title',
                            subtitle: 'customer.blades.member-add.subtitle',
                            controller: 'virtoCommerce.customerModule.memberAddController',
                            template: 'Modules/$(VirtoCommerce.Customer)/Scripts/blades/member-add.tpl.html'
                        };
                        bladeNavigationService.showBlade(newBlade, blade);
                    },
                    canExecuteMethod: function () {
                        return true;
                    },
                    permission: 'customer:create'
                },
                {
                    name: "platform.commands.delete", icon: 'fas fa-trash-alt',
                    executeMethod: function () { deleteList($scope.gridApi.selection.getSelectedRows()); },
                    canExecuteMethod: function () {
                        return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
                    },
                    permission: 'customer:delete'
                },
                {
                    name: "customer.commands.invite-customers", icon: 'fa fa-paper-plane',
                    executeMethod: function () {
                        var selectedOrganization = blade.currentEntity && blade.currentEntity.memberType === 'Organization' ? blade.currentEntity : null;

                        var newBlade = {
                            id: 'inviteCustomers',
                            selectedOrganization: selectedOrganization,
                            controller: 'virtoCommerce.customerModule.inviteCustomersController',
                            template: 'Modules/$(VirtoCommerce.Customer)/Scripts/blades/invite-customers.html'
                        };

                        bladeNavigationService.showBlade(newBlade, blade);

                    },
                    canExecuteMethod: function () {
                        return true;
                    },
                    permission: 'customer:invite',
                    index: 10
                }
            ];


            // filter state for <va-filter-panel>
            var filter = blade.filter = {
                keyword: '',
                memberType: ''
            };

            var dateFieldPrefixes = ['created', 'modified'];
            _.each(dateFieldPrefixes, function (p) {
                filter[p + 'DateRange'] = null;
                filter[p + 'StartDate'] = null;
                filter[p + 'EndDate'] = null;
                filter[p + 'ShowCustomInputs'] = false;
                filter[p + 'CustomStartDate'] = null;
                filter[p + 'CustomEndDate'] = null;
                filter[p + 'CustomRangeApplied'] = false;
            });

            // registered member types come from the resolver service (same source as member-add.js);
            // prepend a synthetic "All" entry so the empty ng-model matches an actual option
            // on first render — an inline <option value=""> would otherwise render blank until
            // the digest cycle catches up with angular-translate.
            filter.memberTypes = [{ memberType: '' }].concat(memberTypesResolverService.objects);

            filter.dateRanges = [
                { value: null, label: 'customer.blades.member-list.labels.filter-date-any' },
                { value: 'today', label: 'customer.blades.member-list.labels.filter-date-today' },
                { value: 'last24h', label: 'customer.blades.member-list.labels.filter-date-last24h' },
                { value: 'last7d', label: 'customer.blades.member-list.labels.filter-date-last7d' },
                { value: 'last30d', label: 'customer.blades.member-list.labels.filter-date-last30d' },
                { value: 'custom', label: 'customer.blades.member-list.labels.filter-date-custom' }
            ];

            filter.formatDate = function (d) {
                if (!d) {
                    return '';
                }
                var dt = d instanceof Date ? d : new Date(d);
                var y = dt.getFullYear();
                var m = ('0' + (dt.getMonth() + 1)).slice(-2);
                var day = ('0' + dt.getDate()).slice(-2);
                return y + '-' + m + '-' + day;
            };

            function applyPreset(prefix, preset) {
                var now = new Date();
                var start = null;
                var end = null;
                switch (preset) {
                    case 'today':
                        start = new Date(now.getFullYear(), now.getMonth(), now.getDate());
                        break;
                    case 'last24h':
                        start = new Date(now.getTime() - 24 * 60 * 60 * 1000);
                        break;
                    case 'last7d':
                        start = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
                        break;
                    case 'last30d':
                        start = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
                        break;
                }
                filter[prefix + 'StartDate'] = start;
                filter[prefix + 'EndDate'] = end;
                filter[prefix + 'ShowCustomInputs'] = false;
                filter[prefix + 'CustomRangeApplied'] = false;
            }

            filter.dateRangeChanged = function (prefix) {
                var preset = filter[prefix + 'DateRange'];
                if (preset === 'custom') {
                    filter[prefix + 'ShowCustomInputs'] = true;
                    return;
                }
                applyPreset(prefix, preset);
                filter.criteriaChanged();
            };

            filter.applyCustomRange = function (prefix) {
                filter[prefix + 'StartDate'] = filter[prefix + 'CustomStartDate'];
                filter[prefix + 'EndDate'] = filter[prefix + 'CustomEndDate'];
                filter[prefix + 'CustomRangeApplied'] = !!(filter[prefix + 'StartDate'] || filter[prefix + 'EndDate']);
                filter[prefix + 'ShowCustomInputs'] = false;
                filter.criteriaChanged();
            };

            filter.editCustomRange = function (prefix) {
                filter[prefix + 'ShowCustomInputs'] = true;
            };

            filter.hasActiveFilters = function () {
                return !!filter.memberType
                    || !!filter.createdStartDate || !!filter.createdEndDate
                    || !!filter.modifiedStartDate || !!filter.modifiedEndDate;
            };

            filter.clearFilters = function () {
                filter.memberType = '';
                _.each(dateFieldPrefixes, function (p) {
                    filter[p + 'DateRange'] = null;
                    filter[p + 'StartDate'] = null;
                    filter[p + 'EndDate'] = null;
                    filter[p + 'ShowCustomInputs'] = false;
                    filter[p + 'CustomStartDate'] = null;
                    filter[p + 'CustomEndDate'] = null;
                    filter[p + 'CustomRangeApplied'] = false;
                });
                filter.criteriaChanged();
            };

            filter.criteriaChanged = function () {
                if ($scope.pageSettings.currentPage > 1) {
                    $scope.pageSettings.currentPage = 1;
                } else {
                    blade.refresh();
                }
            };

            // typing free text → reset column sort so backend relevance ranking applies;
            // clearing the keyword or changing filter panel values leaves the sort alone.
            $scope.$watch('blade.filter.keyword', function (newVal, oldVal) {
                if (newVal === oldVal) {
                    return;
                }
                if (newVal && $scope.gridApi) {
                    _.each($scope.gridApi.grid.columns, function (col) {
                        col.sort = {};
                    });
                }
                filter.criteriaChanged();
            });

            // ui-grid
            $scope.setGridOptions = function (gridId, gridOptions) {
                $scope.gridOptions = gridOptions;
                gridOptionExtension.tryExtendGridOptions(gridId, gridOptions);

                gridOptions.onRegisterApi = function (gridApi) {
                    $scope.gridApi = gridApi;
                    gridApi.core.on.sortChanged($scope, function () {
                        if (!blade.isLoading) blade.refresh();
                    });
                };

                bladeUtils.initializePagination($scope);
            };

            function dateRangeToken(field, start, end) {
                if (!start && !end) {
                    return '';
                }
                return field + ':[' + filter.formatDate(start) + ' TO ' + filter.formatDate(end) + ']';
            }

            function getSearchCriteria() {
                var tokens = [];
                if (filter.keyword) {
                    tokens.push(filter.keyword);
                }
                if (filter.memberType) {
                    tokens.push('membertype:' + filter.memberType);
                }
                var createdToken = dateRangeToken('createddate', filter.createdStartDate, filter.createdEndDate);
                if (createdToken) {
                    tokens.push(createdToken);
                }
                var modifiedToken = dateRangeToken('modifieddate', filter.modifiedStartDate, filter.modifiedEndDate);
                if (modifiedToken) {
                    tokens.push(modifiedToken);
                }
                var composedKeyword = tokens.join(' ');

                return {
                    memberType: blade.memberType,
                    memberId: blade.currentEntity.id,
                    keyword: composedKeyword || undefined,
                    deepSearch: !!composedKeyword,
                    sort: uiGridHelper.getSortExpression($scope),
                    skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                    take: $scope.pageSettings.itemsPerPageCount,
                    objectType: 'Member'
                };
            }


            $scope.copy = function (text) {
                var copyElement = document.createElement("span");
                copyElement.appendChild(document.createTextNode(text));
                copyElement.id = 'tempCopyToClipboard';
                angular.element(document.body.append(copyElement));

                var range = document.createRange();
                range.selectNode(copyElement);
                window.getSelection().removeAllRanges();
                window.getSelection().addRange(range);

                document.execCommand('copy');
                window.getSelection().removeAllRanges();
                copyElement.remove();
            };


            //No need to call this because page 'pageSettings.currentPage' is watched!!! It would trigger subsequent duplicated req...
            //blade.refresh();
        }]);

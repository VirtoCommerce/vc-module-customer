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

            function deleteList(selection) {
                var dialog = {
                    id: "confirmDeleteItem",
                    title: "customer.dialogs.members-delete.title",
                    message: "customer.dialogs.members-delete.message",
                    callback: function (remove) {
                        if (remove) {
                            bladeNavigationService.closeChildrenBlades(blade, function () {
                                blade.isLoading = true;
                                var memberIds = _.pluck(selection, 'id');

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
                dialogService.showConfirmationDialog(dialog);
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
                }
            ];


            // simple and advanced filtering
            var filter = blade.filter = { keyword: null };

            filter.filterByKeyword = function () {
                filter.ignoreSortingForRelevance = uiGridHelper.getSortExpression($scope);
                filter.criteriaChanged();
            };

            filter.criteriaChanged = function () {
                if (filter.keyword === null) {
                    blade.memberType = undefined;
                }
                if ($scope.pageSettings.currentPage > 1) {
                    $scope.pageSettings.currentPage = 1;
                } else {
                    blade.refresh();
                }
            };

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

            function getSearchCriteria() {
                var sortCriteria = uiGridHelper.getSortExpression($scope);

                var searchCriteria = {
                    memberType: blade.memberType,
                    memberId: blade.currentEntity.id,
                    keyword: filter.keyword ? filter.keyword : undefined,
                    deepSearch: filter.keyword ? true : false,
                    sort: filter.keyword && filter.ignoreSortingForRelevance == sortCriteria ? '' : sortCriteria,
                    skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                    take: $scope.pageSettings.itemsPerPageCount,
                    objectType: 'Member'
                };
                return searchCriteria;
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

angular.module('virtoCommerce.customerModule')
    .directive('vcCustomerSearch', ['$localStorage', 'platformWebApp.bladeNavigationService', 'virtoCommerce.customerModule.predefinedSearchFilters', function ($localStorage, bladeNavigationService, predefinedSearchFilters) {
        return {
            restrict: 'E',
            templateUrl: function(elem, attrs) {
                return attrs.templateUrl ||
                    'Modules/$(VirtoCommerce.Customer)/Scripts/directives/customerSearch.tpl.html';
            },
            scope: {
                blade: '='
            },
            link: function($scope) {
                var blade = $scope.blade;
                $scope.$localStorage = $localStorage;
                var filter = $scope.filter = blade.filter;

                if ($localStorage.customerSearchFilterId && !filter.keyword && filter.keyword !== null) {
                    filter.current = _.findWhere($localStorage.customerSearchFilters,
                        { id: $localStorage.customerSearchFilterId });
                    filter.keyword = filter.current ? filter.current.keyword : '';
                    filter.searchInVariations = filter.current ? filter.current.searchInVariations : false;
                }

                filter.change = function(isDetailBladeOpen) {
                    $localStorage.customerSearchFilterId = filter.current ? filter.current.id : null;
                    if (filter.current && !filter.current.id) {
                        filter.current = null;
                        showFilterDetailBlade({ isNew: true });
                    } else {
                        if (!isDetailBladeOpen)
                            bladeNavigationService.closeBlade({ id: 'filterDetail' });
                        filter.keyword = filter.current ? filter.current.keyword : '';
                        filter.searchInVariations = filter.current ? filter.current.searchInVariations : false;
                        filter.criteriaChanged();
                    }
                };

                filter.edit = function($event, entry) {
                    filter.current = entry;
                    showFilterDetailBlade({ data: entry });
                };

                function showFilterDetailBlade(bladeData) {
                    var newBlade = {
                        id: 'filterDetail',
                        controller: 'virtoCommerce.customerModule.filterDetailController',
                        template: 'Modules/$(VirtoCommerce.Customer)/Scripts/blades/filter-detail.tpl.html',
                    };
                    angular.extend(newBlade, bladeData);
                    bladeNavigationService.showBlade(newBlade, blade);
                }

                filter.criteriaChanged();
            }
        };
    }]);

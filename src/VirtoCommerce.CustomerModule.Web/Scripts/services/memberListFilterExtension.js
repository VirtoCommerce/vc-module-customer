angular.module('virtoCommerce.customerModule')
    .factory('virtoCommerce.customerModule.memberListFilterExtensionService', function () {
        var extensions = [];

        return {
            objects: extensions,

            register: function (descriptor) {
                if (!descriptor || !descriptor.id) {
                    throw new Error('memberListFilterExtensionService.register: descriptor.id is required');
                }
                if (!descriptor.templateUrl) {
                    throw new Error('memberListFilterExtensionService.register: descriptor.templateUrl is required (id=' + descriptor.id + ')');
                }
                if (_.findWhere(extensions, { id: descriptor.id })) {
                    throw new Error('memberListFilterExtensionService.register: duplicate id ' + descriptor.id);
                }
                descriptor.priority = _.isNumber(descriptor.priority) ? descriptor.priority : 100;
                extensions.push(descriptor);
            },

            unregister: function (id) {
                if (!id) {
                    throw new Error('memberListFilterExtensionService.unregister: id is required');
                }
                var index = _.findIndex(extensions, { id: id });
                if (index === -1) {
                    return false;
                }
                extensions.splice(index, 1);
                return true;
            },

            clear: function () {
                extensions.length = 0;
            },

            getFilters: function () {
                return _.sortBy(extensions, 'priority');
            }
        };
    });

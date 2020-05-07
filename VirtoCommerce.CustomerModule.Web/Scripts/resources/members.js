angular.module('virtoCommerce.customerModule')
    .factory('virtoCommerce.customerModule.members', ['$resource', function ($resource) {
        return $resource('api/members/:id', {}, {
            search: { method: 'POST', url: 'api/members/search' },
            update: { method: 'PUT' },
            delete: { method: 'POST', url: 'api/members/delete' }
        });
    }])
    .factory('virtoCommerce.customerModule.organizations', ['$resource', function ($resource) {
        return $resource('api/members/organizations', {},
            {
                getByIds: {
                    method: 'POST',
                    url: 'api/organizations/plenty',
                    isArray: true
                },
                search: { method: 'POST', url: 'api/organizations/search' }
            });
    }]);

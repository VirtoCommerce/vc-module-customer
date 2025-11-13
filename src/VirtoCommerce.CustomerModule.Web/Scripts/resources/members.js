angular.module('virtoCommerce.customerModule')
.factory('virtoCommerce.customerModule.members', ['$resource', function ($resource) {
    return $resource('api/members/:id', {}, {
        search: { method: 'POST', url: 'api/members/search' },
        update: { method: 'PUT' },
        get: {
            transformResponse: function (data) {
                const result = angular.fromJson(data);
                if (result.birthDate === '0001-01-01T00:00:00Z') {
                    result.birthDate = null;
                }
                return result;
            }
        },
        delete: { method: 'POST', url: 'api/members/delete' },
        getByUserId: { method: 'GET', url: 'api/members/accounts/:userId' }
    });
}])
.factory('virtoCommerce.customerModule.organizations', ['$resource', function ($resource) {
    return $resource('api/members/organizations', {},
        {
            getByIds: {
                method: 'GET',
                url: 'api/organizations',
                isArray: true
            },
            search: { method: 'POST', url: 'api/organizations/search' }
        });
}]);

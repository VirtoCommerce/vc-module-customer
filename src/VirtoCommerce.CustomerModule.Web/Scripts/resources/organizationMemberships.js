angular.module('virtoCommerce.customerModule')
    .factory('virtoCommerce.customerModule.organizationMemberships', ['$resource', function ($resource) {
        var baseUrl = 'api/customer/organization-memberships';
        return $resource(baseUrl + '/:id', {}, {
            getByUserId: {
                method: 'GET',
                url: baseUrl + '/user/:userId',
                isArray: true,
                params: { userId: '@userId' }
            },
            getByUserAndOrg: {
                method: 'GET',
                url: baseUrl + '/user/:userId/org/:organizationId',
                params: { userId: '@userId', organizationId: '@organizationId' }
            },
            getById: {
                method: 'GET',
                url: baseUrl + '/:id',
                params: { id: '@id' }
            },
            create: {
                method: 'POST',
                url: baseUrl
            },
            update: {
                method: 'PUT',
                url: baseUrl + '/:id',
                params: { id: '@id' }
            },
            delete: {
                method: 'DELETE',
                url: baseUrl,
                isArray: false
            },
            lock: {
                method: 'POST',
                url: baseUrl + '/:id/lock',
                params: { id: '@id' }
            },
            unlock: {
                method: 'POST',
                url: baseUrl + '/:id/unlock',
                params: { id: '@id' }
            }
        });
    }]);

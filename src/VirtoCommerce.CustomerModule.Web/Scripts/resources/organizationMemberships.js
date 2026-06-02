angular.module('virtoCommerce.customerModule')
    .factory('virtoCommerce.customerModule.organizationMemberships', ['$resource', function ($resource) {
        return $resource('api/customer/organization-memberships/:id', {}, {
            getByUserId: {
                method: 'GET',
                url: 'api/customer/organization-memberships/user/:userId',
                isArray: true,
                params: { userId: '@userId' }
            },
            getByUserAndOrg: {
                method: 'GET',
                url: 'api/customer/organization-memberships/user/:userId/org/:organizationId',
                params: { userId: '@userId', organizationId: '@organizationId' }
            },
            getById: {
                method: 'GET',
                url: 'api/customer/organization-memberships/:id',
                params: { id: '@id' }
            },
            create: {
                method: 'POST',
                url: 'api/customer/organization-memberships'
            },
            update: {
                method: 'PUT',
                url: 'api/customer/organization-memberships/:id',
                params: { id: '@id' }
            },
            delete: {
                method: 'DELETE',
                url: 'api/customer/organization-memberships',
                isArray: false
            },
            lock: {
                method: 'POST',
                url: 'api/customer/organization-memberships/:id/lock',
                params: { id: '@id' }
            },
            unlock: {
                method: 'POST',
                url: 'api/customer/organization-memberships/:id/unlock',
                params: { id: '@id' }
            }
        });
    }]);

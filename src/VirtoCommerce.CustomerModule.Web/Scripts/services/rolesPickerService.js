angular.module('virtoCommerce.customerModule')
    .factory('virtoCommerce.customerModule.rolesPickerService', ['platformWebApp.roles', 'platformWebApp.settings',
        function (roles, settings) {
            return {
                create: function (options) {
                    var whitelist = options.whitelistSettingId
                        ? (settings.getValues({ id: options.whitelistSettingId }) || [])
                        : [];
                    var requestToken = 0;
                    var lastFetchedRoles = [];

                    function isRoleAvailable(role) {
                        var selected = (options.getSelectedRoles() || []).map(function (r) {
                            return r.roleId || r.id;
                        });

                        return selected.indexOf(role.id) === -1;
                    }

                    // Keeps the exposed list exactly in sync with the current selection - ui-select resolves
                    // clicks by index into this array, so it must never be filtered again after being rendered.
                    function updateAvailableRoles() {
                        options.onAvailableRolesChanged(lastFetchedRoles.filter(isRoleAvailable));
                    }

                    return {
                        refresh: function (keyword) {
                            requestToken++;
                            var token = requestToken;

                            roles.search({ keyword: keyword || '', take: 20 }).$promise.then(function (data) {
                                if (token !== requestToken) {
                                    return;
                                }

                                var allRoles = data.results || [];

                                if (whitelist.length) {
                                    var whitelistLower = whitelist.map(function (value) {
                                        return value.toLowerCase();
                                    });

                                    allRoles = allRoles.filter(function (r) {
                                        return whitelistLower.indexOf(r.name.toLowerCase()) !== -1;
                                    });
                                }

                                lastFetchedRoles = allRoles;
                                updateAvailableRoles();
                            });
                        },

                        syncAvailableRoles: updateAvailableRoles,

                        normalizeSelected: function (selectedRoles) {
                            for (var i = 0; i < selectedRoles.length; i++) {
                                var r = selectedRoles[i];

                                if (!r.roleId && r.id) {
                                    selectedRoles[i] = { roleId: r.id, roleName: r.name };
                                }
                            }
                        }
                    };
                }
            };
        }
    ]);

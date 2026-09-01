angular.module('virtoCommerce.customerModule')
    .factory('virtoCommerce.customerModule.rolesPickerService', ['platformWebApp.roles', 'platformWebApp.settings', 'platformWebApp.settingsV2',
        function (roles, settings, settingsV2) {
            return {
                create: function (options) {
                    var whitelist = [];

                    if (options.whitelistSettingId) {
                        var globalWhitelist = settings.getValues({ id: options.whitelistSettingId }) || [];
                        whitelist = globalWhitelist;
                        globalWhitelist.$promise.then(reapplyWhitelist);

                        if (options.storeId) {
                            settingsV2.getTenantValues({ tenantType: 'Store', tenantId: options.storeId }).$promise.then(function (values) {
                                var storeValues = (values && values[options.whitelistSettingId]) || [];
                                if (storeValues.length) {
                                    whitelist = storeValues;
                                    reapplyWhitelist();
                                }
                            });
                        }
                    }

                    var requestToken = 0;
                    var lastFetchedRoles = [];
                    var lastUnfilteredRoles = [];

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

                    function filterByWhitelist(allRoles) {
                        if (!whitelist.length) {
                            return allRoles;
                        }

                        var whitelistLower = whitelist.map(function (value) {
                            return value.toLowerCase();
                        });

                        return allRoles.filter(function (r) {
                            return whitelistLower.indexOf(r.name.toLowerCase()) !== -1 ||
                                whitelistLower.indexOf((r.id || '').toLowerCase()) !== -1;
                        });
                    }

                    function reapplyWhitelist() {
                        lastFetchedRoles = filterByWhitelist(lastUnfilteredRoles);
                        updateAvailableRoles();
                    }

                    return {
                        refresh: function (keyword) {
                            requestToken++;
                            var token = requestToken;

                            roles.search({ keyword: keyword || '', take: 20 }).$promise.then(function (data) {
                                if (token !== requestToken) {
                                    return;
                                }

                                lastUnfilteredRoles = data.results || [];
                                lastFetchedRoles = filterByWhitelist(lastUnfilteredRoles);
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

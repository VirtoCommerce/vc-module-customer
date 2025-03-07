angular.module("virtoCommerce.customerModule")
    .controller("virtoCommerce.customerModule.memberIconController", ["$scope", "FileUploader", "platformWebApp.bladeNavigationService", "platformWebApp.dialogService", 'virtoCommerce.customerModule.members', 'platformWebApp.userProfileIconService',
        ($scope, FileUploader, bladeNavigationService, dialogService, membersApi, userProfileIconService) => {
            var blade = $scope.blade;
            blade.title = "customer.blades.member-icon.title";
            blade.saveImmediately = false;

            if (!$scope.iconUploader) {
                const iconUploader = $scope.iconUploader = new FileUploader({
                    scope: $scope,
                    headers: { Accept: "application/json" },
                    autoUpload: true,
                    removeAfterUpload: true,
                    filters: [{
                        name: "imageFilter",
                        fn: (item) => {
                            const approval = /^.*\.(png|jpg|svg|)$/.test(item.name.toLowerCase());
                            if (!approval) {
                                const dialog = {
                                    title: "Filetype error",
                                    message: "Only PNG, JPG or SVG files are allowed."
                                }
                                dialogService.showErrorDialog(dialog);
                            }
                            return approval;
                        }
                    }]
                });

                iconUploader.url = "api/assets?folderUrl=member-icons";

                iconUploader.onSuccessItem = (_, uploadedImages) => {
                    // Need to change icon URL each time to reload image on the blades,
                    // so that we add random postfix to the URL.
                    var postfix = Math.random().toString(36).slice(2, 7);
                    blade.currentEntity.iconUrl = uploadedImages[0].url + "?" + postfix;
                };

                iconUploader.onErrorItem = (element, response, status, _) => {
                    bladeNavigationService.setError(element._file.name + " failed: " + (response.message ? response.message : status), blade);
                };

                iconUploader.onBeforeUploadItem = (item) => {
                    var fileName = item.file.name;
                    var fileNameParts = fileName.split(".");
                    var extension = fileNameParts[fileNameParts.length - 1];

                    // Due not to overwrite icon just on upload we have two files for each contact
                    // and change it on saving.
                    var nameTale = "";
                    if (blade.originalEntity.iconUrl) {
                        var oldUrlParts = blade.originalEntity.iconUrl.split("/");
                        var oldFileName = oldUrlParts[[oldUrlParts.length - 1]];

                        if (!oldFileName.includes("-1")) {
                            nameTale = "-1";
                        }
                    }

                    fileName = blade.currentEntity.id + nameTale + "." + extension;
                    item.file.name = fileName;
                }
            }

            blade.refresh = () => {
                if (blade.originalEntity) {
                    blade.currentEntity = angular.copy(blade.originalEntity);
                    blade.isLoading = false;
                } else {
                    membersApi.get({ id: blade.memberId },
                        function(data) {
                            blade.currentEntity = angular.copy(data);
                            blade.originalEntity = data;
                            blade.saveImmediately = true;
                            blade.isLoading = false;
                        },
                        function(error) { bladeNavigationService.setError('Error ' + error.status, blade); });
                }
            };

            let formScope;
            $scope.setForm = (form) => { formScope = form; }

            $scope.browseFiles = (id) => {
                window.document.querySelector(`#${id}`).click();
            }

            $scope.isValid = false;
            $scope.$watch("blade.currentEntity", function () {
                $scope.isValid = canSave;
            }, true);

            function isDirty() {
                return blade.currentEntity && blade.originalEntity && blade.currentEntity.iconUrl !== blade.originalEntity.iconUrl;
            }

            function canSave() {
                return isDirty() && formScope && formScope.$valid;
            }

            $scope.saveChanges = () => {
                blade.originalEntity.iconUrl = blade.currentEntity.iconUrl;

                if (blade.saveImmediately) {
                    membersApi.update(blade.originalEntity);
                    if (userProfileIconService.userId == blade.currentEntity.id) {
                        userProfileIconService.userIconUrl = blade.currentEntity.iconUrl;
                    }
                }

                $scope.bladeClose();
            };

            $scope.cancelChanges = () => {
                $scope.bladeClose();
            };

            blade.toolbarCommands = [
                {
                    name: "customer.blades.member-icon.labels.reset", icon: "fa fa-undo",
                    executeMethod: () => {
                        blade.currentEntity.iconUrl = blade.originalEntity.iconUrl;
                    },
                    canExecuteMethod: isDirty
                },
                {
                    name: "customer.blades.member-icon.labels.delete", icon: "fas fa-trash-alt",
                    executeMethod: () => {
                        blade.currentEntity.iconUrl = null;
                    },
                    canExecuteMethod: () => blade.currentEntity && blade.originalEntity && blade.currentEntity.iconUrl && blade.originalEntity.iconUrl
                }
            ];

            if (!blade.originalEntity) {
                blade.toolbarCommands.unshift({
                    name: "platform.commands.save", icon: "fas fa-save",
                    executeMethod: $scope.saveChanges,
                    canExecuteMethod: canSave
                });
            }

            blade.refresh();
        }]);

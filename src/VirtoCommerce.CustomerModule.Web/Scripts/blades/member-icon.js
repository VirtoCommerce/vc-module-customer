angular.module("virtoCommerce.customerModule")
    .controller("virtoCommerce.customerModule.memberIconController", ['$rootScope', "$scope", "FileUploader", "platformWebApp.bladeNavigationService", "platformWebApp.dialogService", 'virtoCommerce.customerModule.members',
        ($rootScope, $scope, FileUploader, bladeNavigationService, dialogService, membersApi) => {
            var blade = $scope.blade;
            blade.title = "customer.blades.member-icon.title";

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
                    // blade.tempUrl = uploadedImages[0].url;
                    var postfix = Math.random().toString(36).slice(2, 7);
                    blade.tempUrl = uploadedImages[0].url + "?" + postfix;
                };

                iconUploader.onErrorItem = (element, response, status, _) => {
                    bladeNavigationService.setError(element._file.name + " failed: " + (response.message ? response.message : status), blade);
                };

                iconUploader.onBeforeUploadItem = (item) => {
                    var fileName = item.file.name;
                    var extension = fileName.split(".")[1];

                    // Due not to overwrite icon just on upload we have two files for each contact
                    // and change it on saving.
                    var nameTale = "";
                    if (blade.currentEntity.iconUrl) {
                        var oldUrlParts = blade.currentEntity.iconUrl.split("/");
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
                blade.originalEntity = blade.currentEntity;
                blade.currentEntity = angular.copy(blade.currentEntity);

                blade.isLoading = false;
            };

            let formScope;
            $scope.setForm = (form) => { formScope = form; }

            $scope.browseFiles = (id) => {
                window.document.querySelector(`#${id}`).click();
            }

            function isDirty() {
                return blade.currentEntity.iconUrl !== blade.originalEntity.iconUrl || blade.tempUrl !== blade.currentEntity.iconUrl;
            }

            function canSave() {
                return isDirty() && formScope && formScope.$valid;
            }

            blade.saveChanges = () => {
                blade.currentEntity.iconUrl = blade.tempUrl;
                angular.copy(blade.currentEntity, blade.originalEntity);
                if (blade.saveImmediately) {
                    membersApi.update(blade.currentEntity);
                    $rootScope.$broadcast('memberIconChanged', blade.currentEntity);
                }
                $scope.bladeClose();
            };

            blade.toolbarCommands = [
                {
                    name: "platform.commands.save", icon: "fas fa-save",
                    executeMethod: blade.saveChanges,
                    canExecuteMethod: canSave
                },
                {
                    name: "customer.blades.member-icon.labels.reset", icon: "fa fa-undo",
                    executeMethod: () => {
                        blade.currentEntity.iconUrl = null;
                        blade.tempUrl = null;
                    },
                    canExecuteMethod: () => blade.currentEntity.iconUrl || blade.tempUrl
                }
            ];

            blade.refresh();
        }]);

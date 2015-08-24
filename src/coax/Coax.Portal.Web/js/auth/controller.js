'use strict';

angular.module('Authentication')
    .controller('LoginController',
    [
        '$scope', '$rootScope', '$location', 'AuthenticationService', '$rootScope', '$route',
    function ($scope, $rootScope, $location, AuthenticationService, $route) {
        //$route.reload();
        // reset login status
        AuthenticationService.ClearCredentials();

        $scope.login = function () {
            $scope.dataLoading = true;
            $rootScope.currentUser = $scope.username;
            AuthenticationService.Login($scope.username, $scope.password, function (response) {
                if (response.success) {
                    AuthenticationService.SetCredentials($scope.username, $scope.password);
                    $location.path('/');
                    $rootScope.__authenticated = true;
                } else {
                    $scope.error = response.message;
                    $scope.dataLoading = false;
                }
            });
        };
    }]);


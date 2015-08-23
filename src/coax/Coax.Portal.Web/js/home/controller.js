'use strict';

angular.module('Home')

.controller('HomeController',
    ['$scope', '$cookieStore', '$rootScope', 
    function ($scope, $cookieStore, $rootScope) {
        $scope.currentUser = $cookieStore.get('login_user');
    }]);
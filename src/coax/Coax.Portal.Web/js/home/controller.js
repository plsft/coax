'use strict';

angular.module('Home')

.controller('HomeController',
    ['$scope', '$cookieStore', 
    function ($scope, $cookieStore) {
        $scope.currentUser = $cookieStore.get('login_user');
    }]);
'use strict';

angular.module('404')

.controller('404Controller',
    ['$scope', '$cookieStore',
    function ($scope, $cookieStore) {
        $scope.currentUser = $cookieStore.get('login_user');
    }]);
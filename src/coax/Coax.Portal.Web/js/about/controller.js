'use strict';

angular.module('About')
    .controller('AboutController',
    [
        '$scope', '$cookieStore', '$log',
        function($scope, $cookieStore, $log) {
            $scope.currentUser = $cookieStore.get('login_user');
            $scope.item = {};
            $scope.items = ['item1', 'item2', 'item3'];
            $log.log('test');
        }
    ])
    .controller('TestController', 
    ['$log', function($log) {
        $log.log('in testController');
            }]
    );


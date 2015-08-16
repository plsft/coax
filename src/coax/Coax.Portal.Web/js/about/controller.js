'use strict';

angular.module('About')
    .controller('AboutController',
    [
        '$scope', '$cookieStore', '$log',
        function ($scope, $cookieStore, $log) {
            $scope.currentUser = $cookieStore.get('login_user');
            $scope.item = {};
            $scope.items = ['item1', 'item2', 'item3'];
            $log.log('test');

            $scope.OK = function () {
                $log.log('ok in testController');
            };
        }


    ])
    .controller('TestController', 
    ['$log', '$http',  '$scope', '$interval',
function ($log, $http, $scope, $interval) {
            $log.log('in testContller');
            $http.get('humans.txt')
                .then(function (response) {
                    $interval(function() {
                            $scope.response = response;
                            $log.log('in interval');
                        }, 4000)
                        .finally(function() {
                            $scope.dataLoading = false;
                            $log.log('in finally');
                        });

                }, function() {
                $scope.dataLoading = false;
                $scope.response = "error";
                $log.log('in error');
            });
            
                
        }
        
    ]);


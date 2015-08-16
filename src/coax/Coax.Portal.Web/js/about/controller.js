'use strict';
//
// next test
//http://ui-grid.info/docs/#/tutorial/110_grid_in_modal
//http://stackoverflow.com/questions/25848416/angularjs-ui-grid-render-hyperlink

//controllers bound to page events
// controllers call services 
// services server api returns json

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
    $scope.phones =
         {
             name: 'Nexus S',
             snippet: 'Fast just got faster with Nexus S.',
             age: 1
         };
    $scope.save_phone = function () {
        $log.log('in save_phone ');
        $log.log($scope.phones);
    }

            $http.get('humans.txt')
                .then(function (response) {
                    $interval(function () {
                            $scope.dataLoading = true;
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


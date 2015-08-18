'use strict';
//
// next test
//http://ui-grid.info/docs/#/tutorial/110_grid_in_modal
//http://stackoverflow.com/questions/25848416/angularjs-ui-grid-render-hyperlink
//http://brianhann.com/6-ways-to-take-control-of-how-your-ui-grid-data-is-displayed/
//**

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
    .controller('GridController', ['$log', '$http', '$scope', function($log, $http, $scope) {

        $log.log("in gridController...");
        $scope.myGridData = [
            {
                "firstname": "george",
                "lastname": "rios",
                "company": "plurral"
            },
            {
                "firstname": "john",
                "lastname": "doe",
                "company": "doe enterprises"
            }
        ];

        $scope.gridOptions = {};

        $scope.gridOptions.columnDefs = [
            { name: 'Hyperlink', field: 'id', cellTemplate: '<a data-toggle="modal" role="button" href="#/404?id={{COL_FIELD}}" data-target="#myModal">Open</a>' },
            { name: 'id' },
            { name: 'firstName' },
            { name: 'lastName' }
        ];
        
        $scope.gridOptions.data = [
           {
               "id" : 1,
               "firstName": "Cox",
               "lastName": "Carney",
               "company": "Enormo",
               "employed": true
           },
           {
               "id": 2,
               "firstName": "Lorraine",
               "lastName": "Wise",
               "company": "Comveyer",
               "employed": false
           },
           {
               "id": 3,
               "firstName": "Nancy",
               "lastName": "Waters",
               "company": "Fuelton",
               "employed": false
           }
        ];
    }])
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


'use strict';
//
// next test
//http://ui-grid.info/docs/#/tutorial/110_grid_in_modal
//http://stackoverflow.com/questions/25848416/angularjs-ui-grid-render-hyperlink
//http://brianhann.com/6-ways-to-take-control-of-how-your-ui-grid-data-is-displayed/
//**

//http://www.sitepoint.com/creating-stateful-modals-angularjs-angular-ui-router/
//http://stackoverflow.com/questions/10626885/passing-data-to-a-bootstrap-modal

//https://github.com/fdietz/recipes-with-angular-js-examples/blob/master/chapter8/recipe7/js/app.js
//http://stackoverflow.com/questions/10490570/call-angular-js-from-legacy-code
//controllers bound to page events
// controllers call services 
// services server api returns json

angular.module('About')
    .controller('AboutController',
    [
        '$scope', '$cookieStore', '$log', '$state',
        function ($scope, $cookieStore, $log, $state ) {
            $scope.currentUser = $cookieStore.get('login_user');
            $scope.item = {};
            $scope.items = ['item1', 'item2', 'item3'];
            $log.log('test');
            
            $scope.OK = function () {
                $log.log('ok in testController');
            };

            $scope.opened = function () {
                $log.log('ok in opened.modal');
                $log.log("got coreID:" + localStorage.coreId);
            };
        }
    ])
    .controller('ModalController', ['$log', '$scope', '$state', function($log, $scope, $state) {
        $log.log("in modalController...");
        $log.log("got coreID:" + localStorage.coreId);


    }])
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
            
            //{ name: 'Hyperlink', field: 'id', cellTemplate: '<a data-toggle="modal" data-id="{{COL_FIELD}}" title="Add this item" class="open-AddBookDialog btn btn-primary" href="#/">test</a>' },
            //{ name: 'Hyperlink', field: 'id', cellTemplate: '<button class="btn btn-info" data-core-id="{{COL_FIELD}}" data-toggle="modal" href="#/404?id={{COL_FIELD}}" data-target="#myModal" ui-sref="Modal.success">Open!</button>' },
            { name: 'Hyperlink', field: 'id', cellTemplate: '<button class="btn btn-info" data-core-id="{{COL_FIELD}}" data-toggle="modal" href="#" data-target="#myModal" ng-click="opened()">Open!</button>' },
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


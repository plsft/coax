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


//https://technpol.wordpress.com/2014/08/23/upgrading-to-ng-grid-3-0-ui-grid/
//http://stackoverflow.com/questions/17039926/adding-parameter-to-ng-click-function-inside-ng-repeat-doesnt-seem-to-work
//http://brianhann.com/pass-data-to-a-ui-bootstrap-modal-without-scope/
//controllers bound to page events
// controllers call services 
// services server api returns json

angular.module('Detail')
    .controller('DetailController',
    [
        '$scope', '$cookieStore', '$log', '$state', '$routeParams', '$rootScope',
        function ($scope, $cookieStore, $log, $state, $routeParams, $rootScope) {
            $scope.currentUser = $cookieStore.get('login_user');
        
            $scope.item = {};
            $scope.items = ['item1', 'item2', 'item3'];
            $scope.detail_id = $routeParams.detail_id;
            $scope.order_id = $routeParams.order_id;

            $log.log('test');
            $scope.openedID = null;

            $scope.OK = function () {
                $log.log('ok in detailController');
            };


            
            $scope.open = function (i) {
                $scope.showModal = true;
                $log.log('in open.modal i:' + i);
            };

       

            $scope.openGrid = function (i) {
           $scope.openedID = i;
           $scope.showModal = true;
                $log.log('in open.modal with detail.i: ' + i);
            };

            $scope.ok = function () {
                $scope.showModal = false;
                $log.log('ok in open.modal');
            };

            $scope.cancel = function () {
                $scope.showModal = false;
                $log.log('cancel in open.modal');
            };
        }
    ])
    
    .controller('Grid1Controller', ['$log', '$http', '$scope', function ($log, $http, $scope) {
        $scope.$scope = $scope;
        $scope.openedID = null;

        $log.log("in grid1Controller...");
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
            //{ name: 'Hyperlink', field: 'id', cellTemplate: '<button class="btn btn-info" data-core-id="{{COL_FIELD}}" data-toggle="modal" href="#" data-target="#myModal" ng-click="opened()">Open!</button>' },
            {
                name: 'Hyperlink',
                field: 'id',
                cellTemplate: '<a href="#/detail/{{COL_FIELD}}/{{COL_FIELD}}" class="btn">Open {{COL_FIELD}}</a>'
            },
            { name: 'id' },
            { name: 'firstName' },
            { name: 'lastName' }
        ];


        $scope.openGrid = function (i) {
            $scope.showModal = true;
            $scope.openedID = i;
            $log.log('in open.modal with detail.i: ' + i);
        };

        $scope.ok = function () {
            $scope.showModal = false;
            $log.log('ok in open.modal');
        };

        $scope.cancel = function () {
            $scope.showModal = false;
            $log.log('cancel in open.modal');
        };
        
        $scope.gridOptions.data = [
           {
               "id": 1,
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
    .controller('Test1Controller',
    ['$log', '$http', '$scope', '$interval',
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
                .finally(function () {
                    $scope.dataLoading = false;
                    $log.log('in finally');
                });

        }, function () {
            $scope.dataLoading = false;
            $scope.response = "error";
            $log.log('in error');
        });


}
])

.controller('Main', MainCtrl);
MainCtrl.$inject = ['$modal'];
function MainCtrl($modal) {
    var vm = this;
    vm.people = [
      'Fred',
      'Jim',
      'Bob'
    ];
  
    function deleteModal(person) {
        $modal.open({
            templateUrl: 'js/detail/detail_modal.html',
            controller: ['$modalInstance', 'people', 'person', DeleteModalCtrl],
            controllerAs: 'vm',
            resolve: {
                people: function () { return vm.people },
                person: function() { return person; }
            }
        });
    }

    vm.deleteModal = deleteModal;
}

function DeleteModalCtrl($modalInstance, people, person) {
    var vm = this;
  
    function deletePerson() {
        people.splice(people.indexOf(person), 1);
        $modalInstance.close();
    }

    vm.person = person;
    vm.deletePerson = deletePerson;
  
   
};
'use strict';

// declare modules
angular.module('Authentication', []);
angular.module('Home', []);
angular.module('About', []);
angular.module('Detail', []);
angular.module('404', []);
angular.module('menuController', []);

// declare app
angular.module('CoaxApp', [
        'Authentication',
        'menuController',
        'Home',
        'About',
        '404',
        'Detail',
        'ngRoute',
        'ngCookies',
        'ui.bootstrap',
        'ui.router',
        'angular-loading-bar',
        'ui.grid','ui.grid.cellNav'
    ])
    .config([
        '$routeProvider', function($routeProvider) {
            $routeProvider
                .when('/login', {
                    controller: 'LoginController',
                    templateUrl: 'app/views/login.html',
                    hideMenus: true
                })
                .when('/about', {
                    controller: 'AboutController',
                    templateUrl: 'app/views/about.html'
                })
                .when('/404', {
                    controller: '404Controller',
                    templateUrl: 'app/views/404.html'
                })
                .when('/detail', {
                    controller: 'DetailController',
                    templateUrl: 'app/views/detail.html'
                })
                .when('/detail/:detail_id', {
                    controller: 'DetailController',
                    templateUrl: 'app/views/detail.html'
                })
                 .when('/detail/:detail_id/:order_id', {
                     controller: 'DetailController',
                     templateUrl: 'app/views/detail.html'
                 })
                .when('/', {
                    controller: 'HomeController',
                    templateUrl: 'app/views/home.html'
                })
                .otherwise({ redirectTo: '/login' });
        }
    ])
  
    .run(['$rootScope', '$location', '$cookieStore', '$http', '$state', 
        function ($rootScope, $location, $cookieStore, $http, $state) {
            $rootScope.currentUser = {};

            $rootScope.globals = $cookieStore.get('globals') || {};
            if ($rootScope.globals.currentUser) {
                $http.defaults.headers.common['Authorization'] = 'Basic ' + $rootScope.globals.currentUser.authdata;
                $rootScope.__authenticated = true;                
            }

            $rootScope.state = $state;

            $rootScope.$on('$locationChangeStart', function(event, next, current) {
             if ($location.path() !== '/login' && !$rootScope.globals.currentUser) {
                 $location.path('/login');
                 $rootScope.__authenticated = false;
                }
            });
        }
    ]);


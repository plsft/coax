'use strict';

// declare modules
angular.module('Authentication', []);
angular.module('Home', []);
angular.module('About', []);

angular.module('CoaxApp', [
    'Authentication',
    'Home',
    'About',
    'ngRoute',
    'ngCookies',
    'ui.bootstrap'
])

.config(['$routeProvider', function ($routeProvider) {

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

        .when('/', {
            controller: 'HomeController',
            templateUrl: 'app/views/home.html'
        })

        .otherwise({ redirectTo: '/login' });
}])

.run(['$rootScope', '$location', '$cookieStore', '$http',
    function ($rootScope, $location, $cookieStore, $http) {
        // keep user logged in after page refresh
        $rootScope.globals = $cookieStore.get('globals') || {};
        if ($rootScope.globals.currentUser) {
            $http.defaults.headers.common['Authorization'] = 'Basic ' + $rootScope.globals.currentUser.authdata; // jshint ignore:line
        }

        $rootScope.$on('$locationChangeStart', function (event, next, current) {
            // redirect to login page if not logged in
            if ($location.path() !== '/login' && !$rootScope.globals.currentUser) {
                $location.path('/login');
            }
        });
    }]);
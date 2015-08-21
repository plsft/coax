'use strict';

// declare modules
angular.module('Authentication', []);
angular.module('Home', []);
angular.module('About', []);
angular.module('Detail', []);
angular.module('404', []);

// declare app
angular.module('CoaxApp', [
        'Authentication',
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
    .run([
        '$rootScope', '$location', '$cookieStore', '$http', '$state',
        function($rootScope, $location, $cookieStore, $http, $state) {
            // keep user logged in after page refresh
            $rootScope.globals = $cookieStore.get('globals') || {};
            if ($rootScope.globals.currentUser) {
                $http.defaults.headers.common['Authorization'] = 'Basic ' + $rootScope.globals.currentUser.authdata; // jshint ignore:line
            }

            $rootScope.state = $state;

            $rootScope.$on('$locationChangeStart', function(event, next, current) {
                // redirect to login page if not logged in
                if ($location.path() !== '/login' && !$rootScope.globals.currentUser) {
                    $location.path('/login');
                }
            });
        }
    ]);

angular.module('ui.bootstrap.modal', [])
.constant('modalConfig', {
    backdrop: true,
    escape: true
})
.directive('modal', ['$parse', 'modalConfig', function ($parse, modalConfig) {
    var backdropEl;
    var body = angular.element(document.getElementsByTagName('body')[0]);
    return {
        restrict: 'EA',
        link: function (scope, elm, attrs) {
            var opts = angular.extend({}, modalConfig, scope.$eval(attrs.uiOptions || attrs.bsOptions || attrs.options));
            var shownExpr = attrs.modal || attrs.show;
            var setClosed;

            if (attrs.close) {
                setClosed = function () {
                    scope.$apply(attrs.close);
                };
            } else {
                setClosed = function () {
                    scope.$apply(function () {
                        $parse(shownExpr).assign(scope, false);
                    });
                };
            }
            elm.addClass('modal');

            if (opts.backdrop && !backdropEl) {
                backdropEl = angular.element('<div class="modal-backdrop"></div>');
                backdropEl.css('display', 'none');
                body.append(backdropEl);
            }

            function setShown(shown) {
                scope.$apply(function () {
                    model.assign(scope, shown);
                });
            }

            function escapeClose(evt) {
                if (evt.which === 27) { setClosed(); }
            }
            function clickClose() {
                setClosed();
            }

            function close() {
                if (opts.escape) { body.unbind('keyup', escapeClose); }
                if (opts.backdrop) {
                    backdropEl.css('display', 'none').removeClass('in');
                    backdropEl.unbind('click', clickClose);
                }
                elm.css('display', 'none').removeClass('in');
                body.removeClass('modal-open');
            }
            function open() {
                if (opts.escape) { body.bind('keyup', escapeClose); }
                if (opts.backdrop) {
                    backdropEl.css('display', 'block').addClass('in');
                    if (opts.backdrop != "static") {
                        backdropEl.bind('click', clickClose);
                    }
                }
                elm.css('display', 'block').addClass('in');
                body.addClass('modal-open');
            }

            scope.$watch(shownExpr, function (isShown, oldShown) {
                if (isShown) {
                    open();
                } else {
                    close();
                }
            });
        }
    };
}]);

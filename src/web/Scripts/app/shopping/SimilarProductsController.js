/* jshint -W099 */
/* global jQuery:false */
/* global angular:false */
(function($, productApp) {

    productApp.controller('SimilarProductsController',['$scope', function ($scope) {
        $scope.init = function (id,language) {
            $scope.language = language;
			$scope.visible = false;

            var findIndexId = id + '_' + language;
            $.getJSON('/' + $scope.language + '/api/SimilarProducts/GetSimilarProducts', { contentId: id, _: new Date().getTime().toString() }).success(function (data) {
                $scope.similarProducts = data;
                $scope.visible = typeof ($scope.similarProducts) !== 'undefined' && ($scope.similarProducts !== null && $scope.similarProducts.length > 0);

                $scope.$apply();
            });
        };
        var loadCount = 0;
        $scope.imageLoaded = function() {
            if (loadCount++ === $scope.similarProducts.length - 1) {
                initSimilarProducts();
            }
        }
    }]);

})(jQuery, window.productApp);

function initSimilarProducts() {
    $('#similarProducts').lightSlider({
        item: 4,
        loop: true,
        enableDrag: true,
        currentPagerPosition: 'left',
        autoWidth: false,
        pager: false,
        adaptiveHeight: true,
        responsive: [
            {
                breakpoint: 800,
                settings: {
                    item: 3,
                    slideMove: 1,
                    slideMargin: 6,
                }
            },
            {
                breakpoint: 480,
                settings: {
                    item: 2,
                    slideMove: 1
                }
            }
        ]

});
    
};
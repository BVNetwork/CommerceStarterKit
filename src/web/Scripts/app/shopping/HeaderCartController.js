/* jshint -W099 */
/* global jQuery:false */
/* global angular:false */
(function ($, productApp, commercestarterkit) {

    "use strict";

    productApp.controller("HeaderCartController", ['$scope', 'handleCartService', '$sce', function ($scope, handleCartService, $sce) {

        $scope.accounting = window.accounting;

        $scope.GetCartItemsToHeader = function(language) {
            console.log(language);
            handleCartService.getCart(language, 'Default').$promise.then(function (data) {
                    getCartInfoFromResponse(data);
               
            },
				function () {
				    //console.log(response);
				});

        };

        $scope.formatMoney = function (money) {
            if (!money) {
                money = 0;
            }

            return $scope.accounting.formatMoney(money);

            if ((money + '').indexOf('.') > 0) {
                return money.toFixed(2); // + ' kr';
            }
            return money; // + ' kr';
            //TODO: Fix accoring to currency
            // $scope.currencySymbol
        };
        $scope.removeFromCart = function(product) {
            console.log(product);
            //$scope.trackRemoveFromCart(product);
            handleCartService.remove($scope.language, product, 'Default').$promise.then(function (data) {
                getCartInfoFromResponse(data);
                commercestarterkit.retrieveCartCounters();


            }, function () {

            });

        };

        function getCartInfoFromResponse(data) {
            $scope.cartTotal = data.Total;
            $scope.cartTotalAmount = data.TotalAmount;
            $scope.cartTotalLineItemsAmount = data.TotalLineItemsAmount;
            $scope.cartShipping = data.Shipping;
            $scope.cartTax = data.Tax;
            $scope.cartDiscount = data.Discount;
            $scope.products = data.LineItems;
            $scope.messages = data.Result;
            $scope.discountCodes = data.DiscountCodes;
        }

      

        
    }]);

})(jQuery, window.productApp, window.commercestarterkit);

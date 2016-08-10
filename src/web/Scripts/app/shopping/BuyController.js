/* jshint -W099 */
/* global jQuery:false */
/* global angular:false */
(function($, productApp, commercestarterkit) {

	"use strict";

	productApp.controller("BuyController", ['$scope', 'handleCartService', 'trackingService', function ($scope, handleCartService, trackingService) {

	    $scope.animationImageId = "";
	    $scope.showPopupDialog = false;
        // Used to format numbers

        $scope.initLanguage = function (language) {
	        $scope.language = language;
	    }

        $scope.initWithProduct = function (product) {
            // console.log("Init buy controller with product");
            // console.log(product);

            if(product && product.DisplayName)

            $scope.initLanguage(product.Language);
            $scope.product = $scope.getProductModel(product);
            if (product.Variants) {
                $scope.showPopupDialog = true;
            }
        };

	    $scope.getProductModel = function(fullProductModel) {
	        var product = {
	            code: fullProductModel.Code,
	            name: fullProductModel.Name,
	            quantity: 1,
	            color: '',
	            size: ''
	        };

	        // Defaults
	        if (fullProductModel.Color && fullProductModel.Color.length > 0) {
	            product.color = fullProductModel.Color[0];
	        }
	        if (fullProductModel.Sizes && fullProductModel.Sizes.length > 0) {
	            product.size = fullProductModel.Sizes[0];
	        }
	        return product;
	    };

	    $scope.init = function (language, code, name, quantity, color, size, animationImageId, contentType) {
            $scope.initLanguage(language);
			$scope.product = {
				code: code,
				name: name,
				quantity: quantity,
				color: color,
				size: size
			};
			$scope.animationImageId = animationImageId;
	        // Since we don't have Variants for the simple init, we check the content type
			if (contentType && contentType.lastIndexOf("Fashion", 0) === 0) {
			    $scope.showPopupDialog = true;
			}
        };

		$scope.sanityCheckQuantity = function(quantity) {
			quantity = parseInt(quantity);
			if(quantity < 1 || quantity > 1000) {
				quantity = 1;
			}
			return quantity;
		};

		
		$scope.addToCartDefault = function () {
		    if ($scope.showPopupDialog === true) {
		        commercestarterkit.openProductDialog($scope.product.code);
		    } else {
		        $scope.addToCart($scope.product);
		    }
		};

		$scope.addToCart = function (product) {
			product.quantity = $scope.sanityCheckQuantity(product.quantity);
	        if ($scope.showPopupDialog === true) {
	            commercestarterkit.openProductDialog($scope.product.code);
	        } else {
	            handleCartService.addToCart($scope.language, product);
	            $scope.addedToCartMessageVisible = true;
	            commercestarterkit.updateCartCounter(commercestarterkit.getCartCounter() + parseInt(product.quantity));
	            commercestarterkit.animateCartCount();

	        }
	        // Track in Analytics
		    trackingService.trackAddToCart(product);
		};

		$scope.addToWishlist = function (product) {
		    $scope.loaderVisible = true;
		    $scope.addedToWishlistMessageVisible = false;
		    $scope.addedToCartMessageVisible = false;

		    product.quantity = $scope.sanityCheckQuantity(product.quantity);
		    handleCartService.addToWishlist($scope.language, product);
		    $scope.loaderVisible = false;
		    $scope.addedToWishlistMessageVisible = true;

		    commercestarterkit.updateWishlistCounter(commercestarterkit.getWishlistCounter() + parseInt(product.quantity));
		  
		};

	}]);

})(jQuery, window.productApp, window.commercestarterkit);

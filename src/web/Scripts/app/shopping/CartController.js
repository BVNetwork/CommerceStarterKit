/* jshint -W099 */
/* global jQuery:false */
/* global angular:false */
(function($, productApp, commercestarterkit) {

	"use strict";

	productApp.controller("CartController",['$scope', 'handleCartService', '$sce', function ($scope, handleCartService, $sce) {

		$scope.imageUrl = '';

		$scope.cartName = '';
		$scope.discountCode = '';
		$scope.discountCodes = [];

		$scope.cartTotal = 0;
		$scope.cartTotalAmount = 0;
		$scope.cartTotalLineItemsAmount = 0;
		$scope.cartShipping = 0;
		$scope.cartTax = 0;
		$scope.cartDiscount = 0;
		$scope.products = [];
		$scope.messages = '';
		$scope.loaderVisible = false;
		$scope.addedToCartMessageVisible = false;
		$scope.addedToWishlistMessageVisible = false;
		$scope.copyBillingFields = false;
		$scope.currencySymbol = "TODO";
        // Used to format numbers
		$scope.accounting = window.accounting;

		$scope.initCart = function (name, language) {
		    console.log("Init cart", language, name);
		    $scope.initLanguage(language);
		    $scope.showLoader();
			handleCartService.getCart(language, name).$promise.then(function (data) {
					getCartInfoFromResponse(data);
				},
				function () {
					//console.log(response);
				});

		};

        $scope.initLanguage = function (language) {
	        $scope.language = language;

	        // TODO: Add currency symbol based on current market. We need this from the server
	        if (language == 'no') {
	            $scope.accounting.settings = {
	                currency: {
	                    symbol: "kr",   // default currency symbol is '$'
	                    format: "%v %s", // controls output: %s = symbol, %v = value/number (can be object: see below)
	                    decimal: ",",  // decimal point separator
	                    thousand: ".",  // thousands separator
	                    precision: 2   // decimal places
	                },
	                number: {
	                    precision: 0,  // default precision on numbers is 0
	                    thousand: ".",
	                    decimal: ","
	                }
	            }
	        }
	    }

        $scope.init = function (language, code, name, quantity, imageUrl, colorImageUrl, color, size, description, articleNumber) {
            $scope.initLanguage(language);
			$scope.product = {
				code: code,
				name: name,
				quantity: quantity,
				imageUrl: imageUrl,
				colorImageUrl: colorImageUrl,
				color: color,
				size: size,
				description: description,
				articleNumber: articleNumber
			};
            console.log($scope.product);
        };


		$scope.initShippingAddress = function(streetAddress, zipCode, city, selectedDeliveryLocation) {
			$scope.shippingAddressStreetAddress = streetAddress;
			$scope.shippingAddressPostalCode = zipCode;
			$scope.shippingAddressCity = city;
			$scope.selectedDeliveryLocation = selectedDeliveryLocation;
			if(streetAddress || zipCode || city) {
				$scope.refreshDeliveryLocations();
			}
		};

		$scope.showDeliveryServicePointValidationMessageBoolean = true;

		$scope.showDeliveryServicePointValidationMessage = function () {

			return $scope.showDeliveryServicePointValidationMessageBoolean;
		};

		$scope.refreshDeliveryLocations = function () {

			$scope.showDeliveryServicePointValidationMessageBoolean = false;

			var $deliveryLocations = $("#delivery-locations").empty();

			if ($scope.getDeliveryLocationsTimeout) {
				clearTimeout($scope.getDeliveryLocationsTimeout);

			} else if ($scope.getDeliveryLocations) {
				$scope.getDeliveryLocations.abort();

			}

			$scope.getDeliveryLocationsTimeout = setTimeout(function () {

				delete $scope.getDeliveryLocationsTimeout;

				$scope.getDeliveryLocations = $.get("/api/deliverylocations/get", { streetAddress: $scope.shippingAddressStreetAddress, postalCode: $scope.shippingAddressPostalCode, city: $scope.shippingAddressCity }, function (deliveryLocations) {

					for (var i = 0; i < deliveryLocations.length; ++i) {

						var deliveryLocation = deliveryLocations[i],
							isChecked = i === 0;

						if($scope.selectedDeliveryLocation) {
							isChecked = $scope.selectedDeliveryLocation.Id === deliveryLocation.value.Id;
						}

						$deliveryLocations.append($("<div />", { "class": "form-group" }).
							append($("<label />").text(" " + deliveryLocation.text).
								prepend($("<input />", {
									type: "radio",
									name: "ShippingAddress.DeliveryServicePoint",
									value: JSON.stringify(deliveryLocation.value),
									checked: isChecked
								}))));
					}

					$scope.$apply();

				}).always(function () {

					delete $scope.getDeliveryLocations;

					if ($scope.$root.$$phase !== "$apply" && $scope.$root.$$phase !== "$digest") {
						$scope.$apply();
					}
				});

				$scope.$apply();

			}, 1000);
		};

		$scope.showDeliveryLocationError = function () {

			return !$("#delivery-locations").children().length && !$scope.getDeliveryLocationsTimeout && !$scope.getDeliveryLocations;
		};

		$scope.formatMoney = function(money) {
			if(!money) {
				money = 0;
			}

		    return $scope.accounting.formatMoney(money);

			if((money + '').indexOf('.') > 0) {
				return money.toFixed(2); // + ' kr';
			}
			return money; // + ' kr';
			//TODO: Fix accoring to currency
			// $scope.currencySymbol
		};

		$scope.sanityCheckQuantity = function(quantity) {
			quantity = parseInt(quantity);
			if(quantity < 1 || quantity > 1000) {
				quantity = 1;
			}
			return quantity;
		};

		$scope.addToCart = function(product) {
			$scope.addedToCartMessageVisible = false;
			$scope.addedToWishlistMessageVisible = false;
			$scope.loaderVisible = true;
			product.quantity = $scope.sanityCheckQuantity(product.quantity);
			handleCartService.addToCart($scope.language, product);
			$scope.loaderVisible = false;
			$scope.addedToCartMessageVisible = true;
			commercestarterkit.updateCartCounter(commercestarterkit.getCartCounter() + parseInt(product.quantity));
			commercestarterkit.animateCartCount();
			// Track in Analytics
			$scope.trackAddToCart(product);
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

		$scope.increaseQuantity = function(product) {
		    product.Quantity++;
		    $scope.update(product);
		}
		$scope.decreaseQuantity = function (product) {
		    product.Quantity--;
		    $scope.update(product);
		}

		/**
		 * Update a product in the cart
		 *
		 * @param {object} product
		 * @param {string} [name] - name of cart
		 */
		$scope.update = function(product, name) {
			product.Quantity = $scope.sanityCheckQuantity(product.Quantity);
			$scope.showLoader();
			handleCartService.update($scope.language, product, name).$promise.then(function (data) {
					getCartInfoFromResponse(data);
					commercestarterkit.retrieveCartCounters();
				},
			function () {

			});
		};

		$scope.sizeChanged = function () {
		    var code = $('#sizesId').find(':checked').data('code');
		    $scope.product.code = code;

		};

		/**
		 * Add a discount code to the current cart
		 *
		 * @param {event} event
		 */
		$scope.addDiscountCode = function(event) {
			event.preventDefault();

			if($scope.discountCode && $scope.discountCode !== '') {
				$scope.showLoader();
				handleCartService.addDiscountCode($scope.language, $scope.discountCode).$promise.then(function (data) {
					getCartInfoFromResponse(data);
					commercestarterkit.retrieveCartCounters();
				}, function() {

				});
			}
		};

		/**
		 * Remove a product from the cart
		 *
		 * @param {object} product
		 * @param {event} event
		 * @param {string} [name] - name of cart
		 */
		$scope.removeFromCart = function(product, event, name) {
			if(event) {
				event.preventDefault();
			}
			$scope.showLoader();
			$scope.trackRemoveFromCart(product);
			handleCartService.remove($scope.language, product, name).$promise.then(function (data) {
				getCartInfoFromResponse(data);
				commercestarterkit.retrieveCartCounters();
				

			}, function() {

			});

		};

		$scope.emptyCart = function (name) {
			$scope.showLoader();
			handleCartService.empty($scope.language, name).$promise.then(function (data) {
				getCartInfoFromResponse(data);
				commercestarterkit.retrieveCartCounters();
			}, function () {

			});

		};

		$scope.moveToWishlist = function(product) {
			$scope.showLoader();
			handleCartService.moveToWishlist($scope.language, product).$promise.then(function (data) {
				getCartInfoFromResponse(data);
				commercestarterkit.retrieveCartCounters();
			}, function () {

			});
		};

		$scope.moveToCart = function (product) {
			$scope.showLoader();
			handleCartService.moveToCart($scope.language, product).$promise.then(function (data) {
				getCartInfoFromResponse(data);
				commercestarterkit.retrieveCartCounters();
			}, function () {

			});
		};


		/**
		 * Move all items in the wish list to the cart
		 *
		 * @param name
		 */
		$scope.moveAllToCart = function(name) {
			$scope.showLoader();
			handleCartService.moveAllToCart($scope.language, name).$promise.then(function (data) {
				getCartInfoFromResponse(data);
				commercestarterkit.retrieveCartCounters();
			}, function () {

			});
		};

		$scope.gotoPayment = function(event) {
			if($scope.cartTotalAmount === 0) {
				event.preventDefault();
				return false;
				// TODO: give some response back to the user
			}
		};

		$scope.renderHtml = function(html_code) {
			// not sure if in use - but doesn't seem to work
			return $sce.trustAsHtml(html_code);
		};

		function getCartInfoFromResponse(data) {
			$scope.hideLoader();
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

		$scope.showLoader = function() {
			$scope.loaderVisible = true;
		};

		$scope.hideLoader = function() {
			$scope.loaderVisible = false;
		};


		$scope.copyBillingFieldsToShippingFields = function() {
		    var $shippingAddress = $('#checkoutpage .shipping-address'),
				$billingAddress = $('#checkoutpage .billing-address');
			if($scope.copyBillingFields) {
			    console.log($shippingAddress.find('.firstname').val());
			    console.log($billingAddress.find('.firstname').val());
				$shippingAddress.find('.firstname').val($billingAddress.find('.firstname').val());
				$shippingAddress.find('.lastname').val($billingAddress.find('.lastname').val());

				$scope.shippingAddressStreetAddress = $billingAddress.find('.streetaddress').val();
				$scope.shippingAddressPostalCode = $billingAddress.find('.zipcode').val();
				$scope.shippingAddressCity = $billingAddress.find('.city').val();

				//$scope.refreshDeliveryLocations();

				$shippingAddress.find('input[type=text]').prop('readonly', true);
			} else {
				$shippingAddress.find('input[type=text]').prop('readonly', false);
			}

		};

		$scope.trackAddToCart = function(product) {
			if (window.ga) {
				console.log("Track add to cart", product);
				var thisGa = window.ga;
				thisGa('ec:addProduct', {
					'id': product.code,
					'name': product.name || product.code,
					'quantity': product.quantity
				});
				thisGa('ec:setAction', 'add');
				thisGa('send', 'event', 'UX', 'click', 'add to cart');
			}
		};

		$scope.trackRemoveFromCart = function (product) {
			if (window.ga) {
				console.log("Track remove from cart", product);
				var thisGa = window.ga;
				thisGa('ec:addProduct', {
					'id': product.code,
					'name': product.name || product.code,
					'quantity': product.quantity
				});
				thisGa('ec:setAction', 'remove');
				thisGa('send', 'event', 'UX', 'click', 'remove from cart');
			}
		};

	    //Add(+/-) Button Number Incrementers
		$(".incr-btn").on("click", function (e) {
		    var $button = $(this);
		    var oldValue = $button.parent().find("input").val();
		    if ($button.text() == "+") {
		        $scope.product.quantity++;
		    } else {
		        // Don't allow decrementing below 1
		        if (oldValue > 1) {
		           
		            $scope.product.quantity--;
		        } else {
		           
		        }
		    }		
		    e.preventDefault();
		});

		function animateAddToCart(cart, callback) {
			var imgtodrag = $('#imageslider img').first();

			if (imgtodrag && imgtodrag.length > 0) {
				var imgclone = imgtodrag.clone()
					.offset({
						top: imgtodrag.offset().top,
						left: imgtodrag.offset().left
					})
					.css({
						'opacity': '0.5',
						'position': 'absolute',
						'height': '150px',
						'width': '150px',
						'z-index': '100'
					})
					.appendTo($('body'))
					.animate({
						'top': cart.offset().top,
						'left': cart.offset().left + 30,
						'width': 75,
						'height': 75
					}, 700, 'easeInOutExpo', callback);

				imgclone.animate({
					'width': 0,
					'height': 0
				}, function() {
					imgclone.remove();
				});
			} else {
				callback();
			}
		}
	}]);

})(jQuery, window.productApp, window.commercestarterkit);

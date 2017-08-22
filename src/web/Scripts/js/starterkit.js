/* jshint -W099 */
/* global jQuery:false */

(function($, Oxx, commercestarterkit) {

	"use strict";

	commercestarterkit = window.commercestarterkit = {
        _language : "no",
		_$wishlist: undefined,
		_$wishlistCounter: undefined,
		_$cart: undefined,
		_$cartCounter: undefined,
		_$footerHelpButtons: undefined,

//********************************************************************************
//*PUBLIC FUNCTIONS***************************************************************
//********************************************************************************


		init: function () {
		    this.initImageGallery();
		    this._language = Oxx.AjaxUtils.language;
			this._$wishlist = $('.wishlist-counter');
			this._$wishlistCounter = this._$wishlist.find('.val');
			this._$cart = $('.cart-counter');
			this._$cartCounter = $('.cart-btn a span');//this._$cart.find('.val');
			this._$footerHelpButtons = $('.footer .help');

			this.retrieveCartCounters();
			this.initVideoStarters();
			this.initFooterHelpButtons();
			this.initHotSpotImages();
			
		
			// attach close event to light box close buttons
			$('.lightbox .close').on('click', $.proxy(this._onLightBoxCloseClick, this));

			// change icon on all collapse
			var $collapse = $('.collapse');
			$collapse.on('show.bs.collapse', $.proxy(this._onPanelShown, this));
			$collapse.on('hide.bs.collapse', $.proxy(this._onPanelCollapse, this));
		},

//********************************************************************************

		/**
		 * Set the same height on all elements matching selector
		 * @param selector
		 * @param minHeight
		 * @param padding
		 */
		equalHeight: function (selector, minHeight, padding) {
			var height = minHeight || 0,
				p = padding || 0;
			$(selector).each(function () {
				var h = $(this).height();
				if (h > height) {
					height = h;
				}
			}).css('height', (height + p) + 'px');

		},

//********************************************************************************

		/**
		 * Add events to the video starters
		 */
		initVideoStarters: function() {
			$('.video-container .starter').on('click', $.proxy(this._onVideoStarterClicked, this));
		},

//********************************************************************************

		/**
		 * Set up the HotSpot images
		 */
		initHotSpotImages: function() {

			$('.hotspotimage').each($.proxy(this._iteratorInitEachHotSpotImage, this));


		},

//********************************************************************************

		/**
		 * Show a full screen modal loader
		 */
		showFullScreenLoader: function() {

			var loader = $('<div/>')
				.attr('id', 'full-screen-loader');

			$('body').append(loader);

		},

//********************************************************************************

		/**
		 * Remove the full screen modal loader
		 */
		hideFullScreenLoader: function() {
			$('#full-screen-loader').remove();
		},

//********************************************************************************

		/**
		 * Open up a product as a dialog
		 * @param productId
		 */
		openProductDialog: function(productId) {
			var dialog = new commercestarterkit.ProductDialog(productId);
		},

//********************************************************************************

		/**
		 * Update the wish list counter with provided value
		 * @param {Number} value
		 */
		updateWishlistCounter: function(value) {
			var text = parseInt(value, 10);
			if(text === 0) {
				this._$wishlist.hide();
			} else {
				this._$wishlist.show();
			}
			this._$wishlistCounter.text(text);
		},

//********************************************************************************

		/**
		 * Get the number of items in the wish list
		 * @returns {Number}
		 */
		getWishlistCounter: function() {
			return parseInt(this._$wishlistCounter.text(), 10);
		},

//********************************************************************************

		/**
		 * Update the cart counter with provided value
		 *
		 * @param {Number} value
		 */
		updateCartCounter: function(value) {
			var text = parseInt(value, 10);
			if(text === 0) {
				this._$cart.hide();
			} else {
				this._$cart.show();
			}
			this._$cartCounter.text(text);
			
		},

		animateCartCount: function() {
		    var lineHeight = this._$cartCounter.css('line-height');
		    this._$cartCounter.animate({ fontSize: '250%', lineHeight: '0px' }).animate({ fontSize: '100%', lineHeight: lineHeight });
		    $('.cart-message').addClass('visible');
		    setTimeout(function () {
		        $('.cart-message').removeClass('visible');
		    }, 1500);
		},

//********************************************************************************

		/**
		 * Get the number of items in the cart
		 * @returns {Number}
		 */
		getCartCounter: function() {
			return parseInt(this._$cartCounter.text(), 10);
		},

//********************************************************************************

		/**
		 * Update the counters on the wishlist and the cart links
		 */
		retrieveCartCounters: function() {
			if(this._$cartCounter.length > 0 || this._$wishlistCounter.length > 0) {
			    // TODO: Why not AngularJS Service?
			    $.get('/' + this._language + '/api/cart/getCounters?_=' + new Date().getTime().toString(), undefined,
					$.proxy(this._onRetrieveCartCountersCallback, this), 'json');
			}
		},

//********************************************************************************

		/**
		 * Add click event for internal urls
		 */
		initFooterHelpButtons: function() {

			this._$footerHelpButtons.on('click', $.proxy(this._onHelpClick, this));

		},

//********************************************************************************

		/**
		 *
		 * @param {jQuery} $opener
		 */
		openArticleReference: function($opener) {
			var reference = $opener.data('reference');

			if(reference > 0) {
				$opener.get(0).helpDialog = new commercestarterkit.HelpDialog(reference);
			}
		},

//********************************************************************************
//*CALLBACK METHODS **************************************************************
//********************************************************************************

		/**
		 *
		 * @param data
		 * @private
		 */
		_onRetrieveCartCountersCallback: function(data) {
			if(data) {
				if(typeof(data.wishlist) !== "undefined") {
					this.updateWishlistCounter(data.wishlist);
				}
				if(typeof(data.cart) !== "undefined") {
					this.updateCartCounter(data.cart);
				}
			}
		},

//********************************************************************************

		/**
		 * Init each HotSpot image
		 * @param index
		 * @param item
		 * @private
		 */
		_iteratorInitEachHotSpotImage: function(index, item) {
			var $hotSpotImage = $(item),
				domEl = $hotSpotImage.get(0);

			domEl.HotSpotImage =
				new commercestarterkit.WebComponent.HotSpotImage($hotSpotImage);

			$(domEl.HotSpotImage).on('click', $.proxy(this._onHotSpotClick, this));
		},

//********************************************************************************
//*EVENT METHODS******************************************************************
//********************************************************************************

		/**
		 * When clicking on the image in front of the video, remove the image
		 * and start playing the video
		 *
		 * @param event
		 * @private
		 */
		_onVideoStarterClicked: function(event) {
			event.preventDefault();
			var $starter = $(event.currentTarget);
			if(!$starter.hasClass('disabled')) {
				var $container = $starter.parents('.video-container'),
					$image = $container.find('img'),
					$iframe = $container.find('iframe'),
					$imageText = $container.next();

				$iframe.attr('src', $iframe.data('src'));
				if($imageText.hasClass('image-text')) {
					$imageText.remove();
				}
				if($container.data('open-in-lightbox')) {
					var $lightbox = $('#video-lightbox');

					$lightbox.find('iframe').remove();
					$iframe = $iframe.clone();
					$iframe.appendTo($lightbox.find('.video-container'));

					//$lightbox.find('iframe').attr('width', '99.6%').attr('height', '60%');
					if($lightbox.data('lightbox')) {
						$lightbox.lightbox('show');
					} else {
						$lightbox.lightbox();
						setTimeout(function() {
							$lightbox.find('.lightbox-dialog')
								.position({ of: $lightbox, my: 'center center', at: 'center center', collision: 'fit' });
						}, 200);
					}
				} else {
					$image.remove();
					$starter.remove();
				}
				//$iframe.attr('src', $iframe.data('src'));
			}
		},

//********************************************************************************

		/**
		 *
		 * @param event
		 * @private
		 */
		_onHelpClick: function(event) {
			var $btn = $(event.currentTarget);

			this.openArticleReference($btn);
		},

//********************************************************************************

		/**
		 * Event for closing the full screen video lightbox
		 *
		 * @private
		 */
		_onLightBoxCloseClick: function(event) {
			event.preventDefault();
			event.stopPropagation();
			$(event.currentTarget).parents('.lightbox').lightbox('hide');
		},

		//********************************************************************************

		/**
		 * When panel is opened
		 *
		 * @param event
		 * @private
		 */
		_onPanelShown: function (event) {
			$(event.target).parent().find('.glyphicon').removeClass("glyphicon-chevron-down").addClass("glyphicon-chevron-up");
		},

//********************************************************************************

		/**
		 *
		 * @param event
		 * @private
		 */
		_onPanelCollapse: function (event) {
			$(event.target).parent().find('.glyphicon').removeClass("glyphicon-chevron-up").addClass("glyphicon-chevron-down");
		},

//********************************************************************************

		/**
		 *
		 * @param event
		 * @param productId
		 * @private
		 */
		_onHotSpotClick: function(event, productId) {

			commercestarterkit.openProductDialog(productId);

		},
		initImageGallery: function () {
		    $('.imageGallery').show();
            $('.imageGallery').lightSlider({
                gallery: true,
                item: 1,
                loop: true,
                thumbItem: 3,
                slideMargin: 4,
                enableDrag: false,
                currentPagerPosition: 'left',
                onSliderLoad: function (el) {
                    el.lightGallery({
                        selector: '.imageGallery .lslide'
                    });
                }
            });
            $('.relatedProducts').show();
            $('.relatedProducts').lightSlider({
                item: 2,
                loop: false,
                verticalHeight: 550,
                slideMargin: 5,
                pager: false,
                vertical: true
            });
           
        },

        redirectToUrl: function (data) {
            //console.log(data);
            location.href = data;            
        }
       

	};


    // init commercestarterkit on DOM ready
	$($.proxy(commercestarterkit.init, commercestarterkit));


})(jQuery, window.Oxx, window.commercestarterkit);
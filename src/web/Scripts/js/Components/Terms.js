/* jshint -W099 */
/* global jQuery:false */

(function($, Oxx, commercestarterkit){

	"use strict";

//********************************************************************************
//*NAMESPACES ********************************************************************
//********************************************************************************
	commercestarterkit = window.commercestarterkit = (!commercestarterkit) ? {} : commercestarterkit;

//********************************************************************************
//*CLASS VARIABLES****************************************************************
//********************************************************************************

//********************************************************************************
//*CONSTRUCTOR********************************************************************
//********************************************************************************
	commercestarterkit.Terms = {


//********************************************************************************
//*PROTOTYPE/PUBLIC FUNCTIONS*****************************************************
//********************************************************************************

		/**
		 * Init the checkout page view
		 * @param {string} id
		 */
		init: function(id) {

		    /** @var {jQuery} */
		    this._$el = $((id || '#registration'));

		    if (this._$el.length === 0) {
		        return;
            }

			/** @type {jQuery} */
			this._$termsLink = this._$el.find('.terms a[data-reference]');


			this._initTermsLink();			
		},

//********************************************************************************
//*PRIVATE OBJECT METHODS ********************************************************
//********************************************************************************

		/**
		 * Set up the link on the terms and conditions
		 * @private
		 */
		_initTermsLink: function() {
			if(this._$termsLink.length > 0) {
				this._$termsLink.on('click', $.proxy(this._onTermsClick, this));
			}
		},

//********************************************************************************
//*CALLBACK METHODS **************************************************************
//********************************************************************************


//********************************************************************************
//*EVENT METHODS******************************************************************
//********************************************************************************

		/**
		 * Click on the terms link
		 *
		 * @param event
		 * @private
		 */
		_onTermsClick: function(event) {
			var $btn = $(event.currentTarget);
			commercestarterkit.openArticleReference($btn);
		}


	};


})(jQuery, window.Oxx, window.commercestarterkit);


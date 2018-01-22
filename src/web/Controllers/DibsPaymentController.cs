/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Editor;
using EPiServer.Logging;
using EPiServer.Web.Routing;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Website.Helpers;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Core.Objects.SharedViewModels;
using OxxCommerceStarterKit.Core.PaymentProviders;
using OxxCommerceStarterKit.Core.PaymentProviders.DIBS;
using OxxCommerceStarterKit.Web.Business;
using OxxCommerceStarterKit.Web.Business.Analytics;
using OxxCommerceStarterKit.Web.Business.ClientTracking;
using OxxCommerceStarterKit.Web.Business.Payment;
using OxxCommerceStarterKit.Web.Models.PageTypes.Payment;
using OxxCommerceStarterKit.Web.Models.ViewModels;
using OxxCommerceStarterKit.Web.Models.ViewModels.Payment;
using OxxCommerceStarterKit.Web.Services;
using LineItem = OxxCommerceStarterKit.Core.Objects.LineItem;

namespace OxxCommerceStarterKit.Web.Controllers
{
	public class DibsPaymentController : PaymentBaseController<DibsPaymentPage>
	{
		private readonly IContentRepository _contentRepository;
        private readonly IDibsPaymentProcessor _paymentProcessor;
	    private readonly IIdentityProvider _identityProvider;
	    private readonly IReceiptViewModelBuilder _receiptViewModelBuilder;
	    private readonly IGoogleAnalyticsTracker _googleAnalyticsTracker;
	    private static readonly ILogger _logger = LogManager.GetLogger();
	    private readonly IMetricsLoggingService _metricsLoggingService;

	    public DibsPaymentController(IIdentityProvider identityProvider, 
            IContentRepository contentRepository, 
            IDibsPaymentProcessor paymentProcessor, IReceiptViewModelBuilder receiptViewModelBuilder, 
            IGoogleAnalyticsTracker googleAnalyticsTracker, 
            IMetricsLoggingService metricsLoggingService)
		{
		    _identityProvider = identityProvider;
			_contentRepository = contentRepository;
		    _paymentProcessor = paymentProcessor;
		    _receiptViewModelBuilder = receiptViewModelBuilder;
	        _googleAnalyticsTracker = googleAnalyticsTracker;
		    _metricsLoggingService = metricsLoggingService;
		}

		[RequireSSL]
		public ActionResult Index(DibsPaymentPage currentPage)
		{
			CartHelper ch = new CartHelper(Cart.DefaultName);


            if (ch.IsEmpty && !PageEditing.PageIsInEditMode)
            {                
                return View("Error/_EmptyCartError");
            }

			var orderInfo = new OrderInfo()
			{
                // Dibs expect the order to be without decimals
				Amount = Convert.ToInt32(ch.Cart.Total * 100),
				Currency = ch.Cart.BillingCurrency,
				OrderId = ch.Cart.GeneratePredictableOrderNumber(),
				IsTest = Tools.IsAppSettingTrue("DIBS.TestMode"),
                ExpandOrderInformation = true
			};

            DibsPaymentViewModel model = new DibsPaymentViewModel(_contentRepository, currentPage, orderInfo, ch.Cart);

            // Get cart and track it
            Api.CartController cartApiController = new Api.CartController();
            cartApiController.Language = currentPage.LanguageBranch;
            List<Core.Objects.LineItem> lineItems = cartApiController.GetItems(Cart.DefaultName);
            TrackBeforePayment(lineItems);

			return View(model);
		}

        private void TrackBeforePayment(IEnumerable<LineItem> lineItems)
        {
            // Track Analytics. 
            GoogleAnalyticsTracking tracking = new GoogleAnalyticsTracking(ControllerContext.HttpContext);
            
            // Add the products
            int i = 1;
            foreach (LineItem lineItem in lineItems)
            {
                tracking.ProductAdd(code: lineItem.Code,
                    name: lineItem.Name,
                    quantity: (int)lineItem.Quantity,
                    price: (double)lineItem.PlacedPrice,
                    position: i
                    );
                i++;
            }

            // Step 3 is to pay
            tracking.Action("checkout", "{\"step\":3}");

            // Send it off
            tracking.Custom("ga('send', 'pageview');");
        }

	    /// <summary>
	    /// Processes the payment after we get back from the
	    /// payment provider
	    /// </summary>
	    /// <param name="currentPage"></param>
	    /// <param name="result">The result posted from the provider.</param>
	    [HttpPost]
		[RequireSSL]
		public ActionResult ProcessPayment(DibsPaymentPage currentPage, DibsPaymentResult result)
		{
		    if(_log.IsDebugEnabled())
			    _log.Debug("Payment processed: {0}", result);

	        var model = GetReceiptForPayment(result);

            // Track successfull order in Google Analytics
	        _googleAnalyticsTracker.TrackAfterPayment(model);

            _metricsLoggingService.Count("Purchase", "Purchase Success");
            _metricsLoggingService.Count("Payment", "Dibs");

            return View("ReceiptPage", model);
		}

	    private ReceiptViewModel GetReceiptForPayment(DibsPaymentResult result)
	    {
	        var processingResult = _paymentProcessor.ProcessPaymentResult(result, _identityProvider.GetIdentity());
	        return _receiptViewModelBuilder.BuildFor(processingResult);
	    }

	    [HttpPost]
        [RequireSSL]
        public ActionResult CancelPayment(DibsPaymentPage currentPage, DibsPaymentResult result)
        {
            var model = new CancelPaymentViewModel(currentPage, result);

            _metricsLoggingService.Count("Purchase", "Purchase Cancelled");
            _metricsLoggingService.Count("Payment", "Dibs");

            return View("CancelPayment", model);
        }
    }
}

/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System.Collections.Generic;
using System.Web.Mvc;
using EPiServer;
using Mediachase.Commerce;
using OxxCommerceStarterKit.Core.Objects;
using OxxCommerceStarterKit.Core.Objects.SharedViewModels;
using OxxCommerceStarterKit.Core.Repositories.Interfaces;
using OxxCommerceStarterKit.Core.Services;
using OxxCommerceStarterKit.Web.Models.PageTypes.System;
using OxxCommerceStarterKit.Web.Models.ViewModels;

namespace OxxCommerceStarterKit.Web.Controllers
{
	public class ReceiptPageController : PageControllerBase<ReceiptPage>
	{
        private readonly ICurrentMarket _currentMarket;
	    private readonly IOrderService _orderService;


	    public ReceiptPageController(ICurrentMarket currentMarket, IOrderService orderService)
	    {
	        _currentMarket = currentMarket;
	        _orderService = orderService;	        
	    }

		public ActionResult Index(ReceiptPage currentPage, string order)
		{
			ReceiptViewModel model = new ReceiptViewModel(currentPage);

            model.Order = new OrderViewModel(_currentMarket.GetCurrentMarket().DefaultCurrency.Format);

            if (!string.IsNullOrEmpty(order))
		    {
		        var invoice = _orderService.GetOrderByTrackingNumber(order);
                model.Order = new OrderViewModel(_currentMarket.GetCurrentMarket().DefaultCurrency.Format, invoice);
            }


			// Dummy data to see how the cart looks like in edit            ;
			if (EPiServer.Editor.PageEditing.PageIsInEditMode)
			{
				model.Order.OrderNumber = "PO01234";
				model.Order.TotalAmount = 1234m;
				model.Order.TotalLineItemsAmount = 1234m;
				model.Order.Discount = 0;
				model.Order.Tax = 0.25m * model.Order.TotalAmount;
				model.Order.Shipping = 0;
				model.Order.Discount = 200.0m;
				model.Order.Email = "my@email.org";
				model.Order.Phone = "+47912345678";
				model.Order.BillingAddress = new Address()
				{
					FirstName = "fornavn",
					LastName = "etternavn",
					StreetAddress = "gata 1",
					City = "Oslo",
					ZipCode = "1234"
				};
				model.Order.ShippingAddress = new Address()
				{
					FirstName = "fornavn 2",
					LastName = "etternavn 2",
					StreetAddress = "gata 3",
					City = "Oslo",
					ZipCode = "1235"
				};

				model.Order.OrderLines.Add(new OrderLineViewModel()
				{
					Name = "Produkt",
					Description = "Dette er bare tekst for å se hvordan ting ser ut",
					Price = 1234.0m,
					Quantity = 1,
					Size = "M",
					Color = "Blå"
				});

				model.Order.DiscountCodes = new List<string>() { "sommer2014", "rabattkode2" };

			}

			return View("ReceiptPage", model);
		}	
    }
}

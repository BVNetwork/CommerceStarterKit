/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System.Linq;
using System.Web.Mvc;
using EPiServer.Security;
using Mediachase.Commerce;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Security;
using OxxCommerceStarterKit.Core.Objects.SharedViewModels;
using OxxCommerceStarterKit.Core.Repositories.Interfaces;
using OxxCommerceStarterKit.Core.Services;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using OxxCommerceStarterKit.Web.Models.ViewModels;

namespace OxxCommerceStarterKit.Web.Controllers
{
	public class OrdersPageController : PageControllerBase<OrdersPage>
	{
		private readonly IOrderService _orderService;
	    private readonly ICurrentMarket _currentMarket;

		public OrdersPageController(IOrderService orderService, ICurrentMarket currentMarket)
		{
			_orderService = orderService;
		    _currentMarket = currentMarket;
		}


		// GET: OrdersPage
		public ActionResult Index(OrdersPage currentPage)
		{
			var model = new OrdersPageViewModel(currentPage);
			model.CustomerName = CustomerContext.Current.CurrentContact.FirstName;
            var orders = _orderService.GetOrdersByUserId(EPiServer.Security.PrincipalInfo.CurrentPrincipal.GetContactId());
			model.Orders = orders.OrderByDescending(x => x.Created).Select(CreateOrderViewModel).ToList();
			
			return View(model);
		}


		private OrderViewModel CreateOrderViewModel(PurchaseOrderModel order)
		{
            var model = new OrderViewModel(_currentMarket.GetCurrentMarket().DefaultCurrency.Format, order);
			
            if (order.OrderForms.Any() && order.OrderForms.First().Payments.Any())
            {
                model.PaymentMethod = order.OrderForms.First().Payments.First().PaymentMethodName;
            }

            return model;
		}
	}
}

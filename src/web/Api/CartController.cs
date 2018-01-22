/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Web.Http;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Website.Helpers;
using OxxCommerceStarterKit.Core.Objects;
using OxxCommerceStarterKit.Core.Services;

namespace OxxCommerceStarterKit.Web.Api
{
	public class CartController : BaseApiController
	{

	    private static readonly ILogger _logger; 
		private Injected<ICartService> Cart;

		[HttpGet]
		public List<LineItem> GetItems(string cart)
		{
			return Cart.Service.GetItems(cart, this.Language);
		}

		[HttpGet]
		public CartCountResult GetCounters(string cart = null)
		{
			try
			{
				if (string.IsNullOrEmpty(cart))
				{
					cart = Mediachase.Commerce.Orders.Cart.DefaultName;
				}
				var output = new CartCountResult()
				{
					wishlist = Cart.Service.GetTotal("WishList"),
					cart = Cart.Service.GetTotal(cart)
				};
				return output;
			}
			catch (OperationCanceledException)
			{
				return null;
			}
		}

        [HttpGet]
        public CartResult GetCart(string cart)
        {
            try
            {
                var result = Cart.Service.ValidateCart(cart);
                return GetFullCartResult(cart, result.Message);
            }
            catch (Exception e)
            {
                _logger.Error("Failed to get cart", e);
                Cart.Service.UpdateShipping(cart);
                return new CartResult() {Success = false};
            }            
        }

		[HttpPost]
		public CartResult AddToCart(LineItem product)
		{
			var addToCartResult = Cart.Service.AddToCart(product);
			if (addToCartResult.Success)
			{
				return GetFullCartResult(Mediachase.Commerce.Orders.Cart.DefaultName, addToCartResult.Message);
			}
			else
			{
				return new CartResult() { Result = addToCartResult.Message };
			}
		}

		[HttpPost]
		public CartResult AddToWishList(LineItem product)
		{
			var addToCartResult = Cart.Service.AddToWishList(product);
			if (addToCartResult.Success)
			{
                return GetFullCartResult(CartHelper.WishListName, addToCartResult.Message);
			}
			else
			{
				return new CartResult() { Result = addToCartResult.Message };
			}
		}

		[HttpPost]
		public CartResult Update(LineItem lineItem, string cartName = null)
		{
			if (string.IsNullOrEmpty(cartName))
			{
				cartName = Mediachase.Commerce.Orders.Cart.DefaultName;
			}
			var result = Cart.Service.UpdateCart(cartName, lineItem);
			return GetFullCartResult(cartName, result.Message);
		}

		[HttpPost]
		public CartResult Remove(LineItem product, string cartName = null)
		{
			if (string.IsNullOrEmpty(cartName))
			{
				cartName = Mediachase.Commerce.Orders.Cart.DefaultName;
			}
			var result = Cart.Service.RemoveFromCart(cartName, product);
			return GetFullCartResult(cartName, result.Message);
		}

		[HttpPost]
		public CartResult MoveToWishlist(LineItem product)
		{
			var result = Cart.Service.MoveBetweenCarts(Mediachase.Commerce.Orders.Cart.DefaultName, CartHelper.WishListName, product);
			return GetFullCartResult(Mediachase.Commerce.Orders.Cart.DefaultName, result.Message);
		}

		[HttpPost]
		public CartResult MoveToCart(LineItem product)
		{
			var result = Cart.Service.MoveBetweenCarts(CartHelper.WishListName, Mediachase.Commerce.Orders.Cart.DefaultName, product);
			return GetFullCartResult(CartHelper.WishListName, result.Message);
		}

		[HttpPost]
		public CartResult MoveAllToCart(string name)
		{
			var products = GetItems(name);
			string messages = string.Empty;
			foreach (var product in products)
			{
				var result = Cart.Service.MoveBetweenCarts(CartHelper.WishListName, Mediachase.Commerce.Orders.Cart.DefaultName, product);
				if (!string.IsNullOrEmpty(result.Message))
				{
					messages += result.Message;
				}
			}
			return GetFullCartResult(CartHelper.WishListName, messages);
		}

		[HttpPost]
		public CartResult Empty(string name)
		{
			var result = Cart.Service.EmptyCart(name);
			return GetFullCartResult(name, result.Message);
		}

		[HttpPost]
		public CartResult AddDiscountCode(string code)
		{
			var result = Cart.Service.AddDiscountCode(Mediachase.Commerce.Orders.Cart.DefaultName, code);
			return GetFullCartResult(Mediachase.Commerce.Orders.Cart.DefaultName, result.Message);
		}

		private CartResult GetFullCartResult(string cartName, string message = null)
		{
			var totalAmount = Cart.Service.GetTotalAmount(cartName);
			var result =  new CartResult()
			{
				Success = true,
				Result = message,
				Total = Cart.Service.GetTotal(cartName),
				TotalAmount = totalAmount,
				TotalLineItemsAmount = Cart.Service.GetTotalLineItemsAmount(cartName),
				Tax = Cart.Service.GetTax(cartName),
				Shipping = Cart.Service.GetShipping(cartName),
				Discount = Cart.Service.GetTotalDiscount(cartName),
				LineItems = Cart.Service.GetItems(cartName, this.Language),
				DiscountCodes = Cart.Service.GetAllDiscountCodes(cartName)
			};

			return result;
		}
	}

	public class CartResult
	{
		public bool Success { get; set; }
		public string Result { get; set; }
		public decimal Total { get; set; }
		public decimal TotalAmount { get; set; }
		public decimal TotalLineItemsAmount { get; set; }
		public decimal Tax { get; set; }
		public decimal Shipping { get; set; }
		public decimal Discount { get; set; }
		public List<LineItem> LineItems { get; set; }
		public List<DiscountItem> DiscountCodes { get; set; }
	}

	public class CartCountResult
	{
		public decimal wishlist { get; set; }
		public decimal cart { get; set; }
	}

}

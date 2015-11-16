/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Objects;
using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Marketing;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Website.Helpers;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Core.Objects;
using LineItem = OxxCommerceStarterKit.Core.Objects.LineItem;
using OxxCommerceStarterKit.Core.Objects.SharedViewModels;

namespace OxxCommerceStarterKit.Core.Services
{
    public class CartService : ICartService
    {
        private readonly IContentLoader _contentLoader;
        private readonly ReferenceConverter _referenceConverter;

        public CartService(IContentLoader contentLoader, ReferenceConverter referenceConverter)
        {
            _contentLoader = contentLoader;
            _referenceConverter = referenceConverter;
        }

        public CartActionResult AddToCart(LineItem lineItem)
        {
            return AddToCart(Cart.DefaultName, lineItem);
        }

        public CartActionResult AddToWishList(LineItem lineItem)
        {
            return AddToCart(CartHelper.WishListName, lineItem);
        }

        private CartActionResult AddToCart(string name, LineItem lineItem)
        {
            CartHelper ch = new CartHelper(name);
            string messages = string.Empty;

            if (lineItem.Quantity < 1)
            {
                lineItem.Quantity = 1;
            }

            // Need entry for adding to cart
            var entry = CatalogContext.Current.GetCatalogEntry(lineItem.Code);
            ch.AddEntry(entry, lineItem.Quantity, false, new CartHelper[] { });

            // Need content for easier access to more information
            ContentReference itemLink = _referenceConverter.GetContentLink(entry.CatalogEntryId,
                CatalogContentType.CatalogEntry, 0);
            EntryContentBase entryContent = _contentLoader.Get<EntryContentBase>(itemLink);

            // Populate line item with as much as we can find
            if (string.IsNullOrEmpty(lineItem.ImageUrl))
            {
                lineItem.ImageUrl = entryContent.GetDefaultImage();
            }

            if (string.IsNullOrEmpty(lineItem.ArticleNumber))
            {
                lineItem.ArticleNumber = entry.ID;
            }

            lineItem.Name = TryGetDisplayName(entry);
            //lineItem.IsInventoryAllocated = true; // Will this be enough?

            AddCustomProperties(lineItem, ch.Cart);


            messages = RunWorkflowAndReturnFormattedMessage(ch.Cart, OrderGroupWorkflowManager.CartValidateWorkflowName);
            ch.Cart.AcceptChanges();

            // TODO: Always returns success, if we get warnings, we need to show them
            return new CartActionResult() { Success = true, Message = messages };
        }

        public static string RunWorkflowAndReturnFormattedMessage(Cart cart, string workflowName)
        {
            string returnString = string.Empty;

            // TODO: Be aware of this magic string that the workflow requires
            cart.ProviderId = "FrontEnd";
            WorkflowResults results = cart.RunWorkflow(workflowName);
            var resultsMessages = OrderGroupWorkflowManager.GetWarningsFromWorkflowResult(results);
            if (resultsMessages.Count() > 0)
            {
                returnString = "";
                foreach (string result in resultsMessages)
                {
                    returnString += result + "<br />";
                }
            }
            return returnString;
        }

        private string TryGetDisplayName(Entry entry)
        {
            if (entry.ItemAttributes["DisplayName"] != null &&
                entry.ItemAttributes["DisplayName"].Value != null &&
                !string.IsNullOrEmpty(entry.ItemAttributes["DisplayName"].Value.First()))
            {
                return entry.ItemAttributes["DisplayName"].Value.First().Trim();
            }

            return entry.Name;
        }

        public List<LineItem> GetItems(string cart, string language)
        {
            CartHelper ch = new CartHelper(cart);
            List<LineItem> items = new List<LineItem>();
            if (ch.LineItems != null)
            {
                foreach (Mediachase.Commerce.Orders.LineItem lineItem in ch.LineItems)
                {
                    var item = new LineItem(lineItem, language);
                    item.UpdateData(lineItem);
                    items.Add(item);
                }
            }

            return items;
        }

        public CartActionResult UpdateCart(string name, LineItem product)
        {
            CartHelper ch = new CartHelper(name);
            string messages = string.Empty;

            var item = ch.LineItems.FirstOrDefault(i => i.Code == product.Code);

            if (item != null)
            {
                item.Quantity = product.Quantity > 0 ? product.Quantity : 0;

                messages = RunWorkflowAndReturnFormattedMessage(ch.Cart, OrderGroupWorkflowManager.CartValidateWorkflowName);

                ch.Cart.AcceptChanges();
            }

            // TODO: Should we always return success? What if the messages indicate otherwise?
            return new CartActionResult() { Success = true, Message = messages };
        }

        public decimal GetTotal(string name)
        {
            return new CartHelper(name).LineItems.Sum(l => l.Quantity);
        }

        public decimal GetTotalAmount(string name)
        {
            return new CartHelper(name).Cart.Total;//.LineItems.Sum(l => (l.Quantity * l.PlacedPrice) - l.LineItemDiscountAmount - l.OrderLevelDiscountAmount);
        }

        public decimal GetTotalLineItemsAmount(string name)
        {
            return new CartHelper(name).Cart.SubTotal;//.LineItems.Sum(l => l.Quantity * l.PlacedPrice);
        }

        public decimal GetTotalDiscount(string name)
        {
            return new CartHelper(name).LineItems.Sum(l => l.LineItemDiscountAmount + l.OrderLevelDiscountAmount);
        }

        public decimal GetTax(string name)
        {
            var cart = new CartHelper(name).Cart;
            var tax = cart.TaxTotal;
            // TODO: Get tax percent from somewhere
            if (tax == 0)
            {
                tax = 0.25m * cart.Total;
            }
            return tax;
        }

        public decimal GetShipping(string name)
        {
            return new CartHelper(name).Cart.ShippingTotal;
        }

        public CartActionResult RemoveFromCart(string name, LineItem product)
        {
            CartHelper ch = new CartHelper(name);
            var item = ch.Cart.OrderForms[0].LineItems.FindItemByCatalogEntryId(product.Code);
            ch.Cart.OrderForms[0].LineItems.Remove(item);
            ch.Cart.AcceptChanges();

            string messages = RunWorkflowAndReturnFormattedMessage(ch.Cart, OrderGroupWorkflowManager.CartValidateWorkflowName);

            return new CartActionResult() { Success = true, Message = messages };
        }

        public CartActionResult MoveBetweenCarts(string fromName, string toName, LineItem product)
        {
            var result1 = RemoveFromCart(fromName, product);
            if (result1.Success)
            {
                return AddToCart(toName, product);
            }
            return result1;
        }

        public CartActionResult EmptyCart(string name)
        {
            CartHelper ch = new CartHelper(name);
            ch.Delete();
            ch.Cart.AcceptChanges();
            return new CartActionResult() { Success = true };
        }

        public CartActionResult ValidateCart(string name)
        {
            CartHelper ch = new CartHelper(name);
            CartActionResult actionResult = new CartActionResult()
            {
                Success = false,
                Message = ""
            };

            if (ch.IsEmpty == false)
            {
                var cart = ch.Cart;

                actionResult.Message = RunWorkflowAndReturnFormattedMessage(cart, OrderGroupWorkflowManager.CartValidateWorkflowName);
                cart.AcceptChanges();
                actionResult.Success = true;
            }
            return actionResult;
        }

        public CartActionResult AddDiscountCode(string name, string code)
        {
            CartHelper ch = new CartHelper(name);
            string messages = string.Empty;
            bool success = true;

            var localizationService = ServiceLocator.Current.GetInstance<LocalizationService>();

            var discounts = GetAllDiscounts(ch.Cart);
            if (discounts.Exists(x => x.DiscountCode == code))
            {
                if (!string.IsNullOrEmpty(messages))
                {
                    messages += "  ";
                }
                messages += localizationService.GetString("/common/cart/coupon_codes/error_already_used");
            }
            else
            {

                MarketingContext.Current.AddCouponToMarketingContext(code);

                if (!ch.IsEmpty)
                {
                    messages += ValidateCart(name).Message;

                    // check if coupon was applied
                    discounts = GetAllDiscounts(ch.Cart);



                    if (discounts.Count == 0 || !discounts.Exists(x => x.DiscountCode == code))
                    {
                        success = false;
                        if (!string.IsNullOrEmpty(messages))
                        {
                            messages += "  ";
                        }

                        messages += localizationService.GetString("/common/cart/coupon_codes/error_invalid");
                    }

                }
            }
            return new CartActionResult() { Success = success, Message = messages };
        }

        public void UpdateShipping(string name)
        {
            var cart = new CartHelper(name).Cart;
            cart.OrderForms[0].SetShipmentLineItemQuantity();
            cart.AcceptChanges();
        }

        private void AddCustomProperties(LineItem lineItem, Cart cart)
        {

            Mediachase.Commerce.Orders.LineItem item = cart.OrderForms[0].LineItems.FindItemByCatalogEntryId(lineItem.Code);

            // Make sure we have all available data on the item before
            // we proceed
            lineItem.UpdateData(item);

            //TODO: Let specific model implementation populate these fields, we need to know too much about the model here
            item[Constants.Metadata.LineItem.DisplayName] = lineItem.Name;
            item[Constants.Metadata.LineItem.ImageUrl] = lineItem.ImageUrl;
            item[Constants.Metadata.LineItem.Size] = lineItem.Size;
            item[Constants.Metadata.LineItem.Description] = lineItem.Description;
            item[Constants.Metadata.LineItem.Color] = lineItem.Color;
            item[Constants.Metadata.LineItem.ColorImageUrl] = lineItem.ColorImageUrl;
            item[Constants.Metadata.LineItem.ArticleNumber] = lineItem.ArticleNumber;

            cart.AcceptChanges();
        }

        public List<DiscountItem> GetAllDiscountCodes(string name)
        {
            var cart = new CartHelper(name).Cart;
            var discounts = GetAllDiscounts(cart);
            return discounts.Where(x => !string.IsNullOrEmpty(x.DiscountCode)).Select(x => new DiscountItem(x)).ToList();
        }

        public static List<Discount> GetAllDiscounts(OrderGroup cart)
        {
            var discounts = new List<Discount>();
            foreach (OrderForm form in cart.OrderForms)
            {
                foreach (var discount in form.Discounts.Cast<Discount>().Where(x => !String.IsNullOrEmpty(x.DiscountCode)))
                {
                    AddToDiscountList(discount, discounts);
                }

                foreach (Mediachase.Commerce.Orders.LineItem item in form.LineItems)
                {
                    foreach (var discount in item.Discounts.Cast<Discount>().Where(x => !String.IsNullOrEmpty(x.DiscountCode)))
                    {
                        AddToDiscountList(discount, discounts);
                    }
                }

                foreach (Shipment shipment in form.Shipments)
                {
                    foreach (var discount in shipment.Discounts.Cast<Discount>().Where(x => !String.IsNullOrEmpty(x.DiscountCode)))
                    {
                        AddToDiscountList(discount, discounts);
                    }
                }
            }
            return discounts;
        }

        public static List<DiscountModel> GetAllDiscounts(PurchaseOrderModel order)
        {
            var discounts = new List<DiscountModel>();
            foreach (var form in order.OrderForms)
            {
                foreach (var discount in form.Discounts.Where(x => !String.IsNullOrEmpty(x.DiscountCode)))
                {
                    AddToDiscountList(discount, discounts);
                }

                foreach (var item in form.LineItems)
                {
                    foreach (var discount in item.Discounts.Where(x => !String.IsNullOrEmpty(x.DiscountCode)))
                    {
                        AddToDiscountList(discount, discounts);
                    }
                }

                foreach (var shipment in form.Shipments)
                {
                    foreach (var discount in shipment.Discounts.Where(x => !String.IsNullOrEmpty(x.DiscountCode)))
                    {
                        AddToDiscountList(discount, discounts);
                    }
                }
            }
            return discounts;
        }

        public static void AddToDiscountList(Discount discount, List<Discount> discounts)
        {
            if (!discounts.Exists(x => x.DiscountCode.Equals(discount.DiscountCode)))
            {
                discounts.Add(discount);
            }
        }

        public static void AddToDiscountList(DiscountModel discount, List<DiscountModel> discounts)
        {
            if (!discounts.Exists(x => x.DiscountCode.Equals(discount.DiscountCode)))
            {
                discounts.Add(discount);
            }
        }
    }
}

using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Objects;
using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Marketing;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Website.Helpers;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Core.Objects;
using OxxCommerceStarterKit.Core.Objects.SharedViewModels;
using LineItem = OxxCommerceStarterKit.Core.Objects.LineItem;

namespace OxxCommerceStarterKit.Core.Services
{
    [ServiceConfiguration(typeof(ICartService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class CartService : ICartService
    {
        private readonly IPricingService _pricingService;
        private readonly IOrderFactory _orderFactory;
        private readonly ICurrentCustomerService _customerContext;
        private readonly IPlacedPriceProcessor _placedPriceProcessor;
        private readonly IInventoryProcessor _inventoryProcessor;
        private readonly ILineItemValidator _lineItemValidator;
        private readonly IPromotionEngine _promotionEngine;
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentMarket _currentMarket;
        private readonly IContentLoader _contentLoader;
        private readonly ReferenceConverter _referenceConverter;

        public CartService(
            IPricingService pricingService,
            IOrderFactory orderFactory,
            ICurrentCustomerService customerContext,
            IPlacedPriceProcessor placedPriceProcessor,
            IInventoryProcessor inventoryProcessor,
            ILineItemValidator lineItemValidator,
            IOrderRepository orderRepository,
            IPromotionEngine promotionEngine,
            ICurrentMarket currentMarket,
            IContentLoader contentLoader, 
            ReferenceConverter referenceConverter
            )
        {
            _pricingService = pricingService;
            _orderFactory = orderFactory;
            _customerContext = customerContext;
            _placedPriceProcessor = placedPriceProcessor;
            _inventoryProcessor = inventoryProcessor;
            _lineItemValidator = lineItemValidator;
            _promotionEngine = promotionEngine;
            _orderRepository = orderRepository;
            _currentMarket = currentMarket;
            _contentLoader = contentLoader;
            _referenceConverter = referenceConverter;
        }

        public string DefaultCartName
        {
            get { return "Default"; }
        }

        public string DefaultWishListName
        {
            get { return "WishList"; }
        }

        public void MergeShipments(ICart cart)
        {
            if (cart == null || !cart.GetAllLineItems().Any())
            {
                return;
            }

            var form = cart.GetFirstForm();
            var keptShipment = cart.GetFirstShipment();
            var removedShipments = form.Shipments.Skip(1).ToList();
            var movedLineItems = removedShipments.SelectMany(x => x.LineItems).ToList();
            removedShipments.ForEach(x => x.LineItems.Clear());
            removedShipments.ForEach(x => cart.GetFirstForm().Shipments.Remove(x));

            foreach (var item in movedLineItems)
            {
                var existingLineItem = keptShipment.LineItems.SingleOrDefault(x => x.Code == item.Code);
                if (existingLineItem != null)
                {
                    existingLineItem.Quantity += item.Quantity;
                    continue;
                }

                keptShipment.LineItems.Add(item);
            }

            ValidateCart(cart);
        }


        public bool AddToCart(ICart cart, string code, int quantity, out string warningMessage)
        {
            warningMessage = string.Empty;

            var lineItem = cart.GetAllLineItems().FirstOrDefault(x => x.Code == code);

            if (lineItem == null)
            {
                lineItem = _orderFactory.CreateLineItem(code);
                lineItem.Quantity = quantity;
                cart.AddLineItem(lineItem, _orderFactory);
            }
            else
            {
                var shipment = cart.GetFirstShipment();
                cart.UpdateLineItemQuantity(shipment, lineItem, lineItem.Quantity + quantity);
            }

            var validationIssues = ValidateCart(cart);

            foreach (var validationIssue in validationIssues)
            {
                warningMessage += String.Format("Line Item with code {0} ", lineItem.Code);
                warningMessage = validationIssue.Value.Aggregate(warningMessage, (current, issue) => current + String.Format("{0}, ", issue));
                warningMessage = warningMessage.Substring(0, warningMessage.Length - 2);
            }

            if (validationIssues.HasItemBeenRemoved(lineItem))
            {
                return false;
            }

            return GetFirstLineItem(cart, code) != null;
        }

        public void SetCartCurrency(ICart cart, Currency currency)
        {
            if (currency.IsEmpty || currency == cart.Currency)
            {
                return;
            }

            cart.Currency = currency;
            foreach (var lineItem in cart.GetAllLineItems())
            {
                //If there is an item which has no price in the new currency, a NullReference exception will be thrown.
                //Mixing currencies in cart is not allowed.
                //It's up to site's managers to ensure that all items have prices in allowed currency.
                lineItem.PlacedPrice = _pricingService.GetPrice(lineItem.Code, cart.Market.MarketId, currency).Value.Amount;
            }

            ValidateCart(cart);
        }

        public Dictionary<ILineItem, List<ValidationIssue>> ValidateCart(ICart cart)
        {
            if (cart.Name.Equals(DefaultWishListName))
            {
                return new Dictionary<ILineItem, List<ValidationIssue>>();
            }

            var validationIssues = new Dictionary<ILineItem, List<ValidationIssue>>();
            cart.ValidateOrRemoveLineItems((item, issue) => validationIssues.AddValidationIssues(item, issue), _lineItemValidator);
            cart.UpdatePlacedPriceOrRemoveLineItems(_customerContext.GetContactById(cart.CustomerId), (item, issue) => validationIssues.AddValidationIssues(item, issue), _placedPriceProcessor);
            cart.UpdateInventoryOrRemoveLineItems((item, issue) => validationIssues.AddValidationIssues(item, issue), _inventoryProcessor);

            cart.ApplyDiscounts(_promotionEngine, new PromotionEngineSettings());

            return validationIssues;
        }

        public Dictionary<ILineItem, List<ValidationIssue>> RequestInventory(ICart cart)
        {
            var validationIssues = new Dictionary<ILineItem, List<ValidationIssue>>();
            cart.AdjustInventoryOrRemoveLineItems((item, issue) => validationIssues.AddValidationIssues(item, issue), _inventoryProcessor);
            return validationIssues;
        }

        public ICart LoadCart(string name)
        {
            var cart = _orderRepository.LoadCart<ICart>(_customerContext.GetCurrentUserGuid(), name, _currentMarket);
            if (cart != null)
            {
                SetCartCurrency(cart, _currentMarket.GetCurrentMarket().DefaultCurrency);

                var validationIssues = ValidateCart(cart);
                // After validate, if there is any change in cart, saving cart.
                if (validationIssues.Any())
                {
                    _orderRepository.Save(cart);
                }
            }

            return cart;
        }

        public ICart LoadOrCreateCart(string name)
        {
            var cart = _orderRepository.LoadOrCreateCart<ICart>(_customerContext.GetCurrentUserGuid(), name, _currentMarket);
            if (cart != null)
            {
                SetCartCurrency(cart, _currentMarket.GetCurrentMarket().DefaultCurrency);
            }

            return cart;
        }

        public bool AddCouponCode(ICart cart, string couponCode)
        {
            var couponCodes = cart.GetFirstForm().CouponCodes;
            if (couponCodes.Any(c => c.Equals(couponCode, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            couponCodes.Add(couponCode);
            var rewardDescriptions = cart.ApplyDiscounts(_promotionEngine, new PromotionEngineSettings());
            var appliedCoupons = rewardDescriptions.Where(r => r.Status == FulfillmentStatus.Fulfilled && !string.IsNullOrEmpty(r.Promotion.Coupon.Code))
                                                   .Select(c => c.Promotion.Coupon.Code);
            var couponApplied = appliedCoupons.Any(c => c.Equals(couponCode, StringComparison.OrdinalIgnoreCase));
            if (!couponApplied)
            {
                couponCodes.Remove(couponCode);
            }
            return couponApplied;
        }

        public void RemoveCouponCode(ICart cart, string couponCode)
        {
            cart.GetFirstForm().CouponCodes.Remove(couponCode);
            cart.ApplyDiscounts(_promotionEngine, new PromotionEngineSettings());
        }

        private void RemoveLineItem(ICart cart, int shipmentId, string code)
        {
            var shipment = cart.GetFirstForm().Shipments.First(s => s.ShipmentId == shipmentId || shipmentId <= 0);

            var lineItem = shipment.LineItems.FirstOrDefault(l => l.Code == code);
            if (lineItem != null)
            {
                shipment.LineItems.Remove(lineItem);
            }

            if (!shipment.LineItems.Any())
            {
                cart.GetFirstForm().Shipments.Remove(shipment);
            }

            ValidateCart(cart);
        }

        private void UpdateLineItemSku(ICart cart, int shipmentId, string oldCode, string newCode, decimal quantity)
        {
            RemoveLineItem(cart, shipmentId, oldCode);

            //merge same sku's
            var newLineItem = GetFirstLineItem(cart, newCode);
            if (newLineItem != null)
            {
                var shipment = cart.GetFirstForm().Shipments.First(s => s.ShipmentId == shipmentId || shipmentId <= 0);
                cart.UpdateLineItemQuantity(shipment, newLineItem, newLineItem.Quantity + quantity);
            }
            else
            {
                newLineItem = _orderFactory.CreateLineItem(newCode);
                newLineItem.Quantity = quantity;
                cart.AddLineItem(newLineItem, _orderFactory);

                var price = _pricingService.GetCurrentPrice(newCode);
                if (price.HasValue)
                {
                    newLineItem.PlacedPrice = price.Value.Amount;
                }
            }

            ValidateCart(cart);
        }

        private void ChangeQuantity(ICart cart, int shipmentId, string code, decimal quantity)
        {
            if (quantity == 0)
            {
                RemoveLineItem(cart, shipmentId, code);
            }
            var shipment = cart.GetFirstForm().Shipments.First(s => s.ShipmentId == shipmentId || shipmentId <= 0);
            var lineItem = shipment.LineItems.FirstOrDefault(x => x.Code == code);
            if (lineItem == null)
            {
                return;
            }

            cart.UpdateLineItemQuantity(shipment, lineItem, quantity);
            ValidateCart(cart);
        }

        private ILineItem GetFirstLineItem(ICart cart, string code)
        {
            return cart.GetAllLineItems().FirstOrDefault(x => x.Code == code);
        }

        // Oldie
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
            string code = lineItem.Code;

            if (lineItem.Quantity < 1)
            {
                lineItem.Quantity = 1;
            }

            string messages = string.Empty;
            ICart cart = LoadOrCreateCart(DefaultCartName);
            var result = AddToCart(cart, code, lineItem.Quantity, out messages);

            // Populate with additional fields before saving
            ILineItem addedLineItem = cart.GetAllLineItems().FirstOrDefault(x => x.Code == code);
            if (addedLineItem != null)
            {
                // Need content for easier access to more information
                ContentReference itemLink = _referenceConverter.GetContentLink(code);
                EntryContentBase entryContent = _contentLoader.Get<EntryContentBase>(itemLink);

                AddPropertiesToLineItem(addedLineItem, lineItem, entryContent);

                AddCustomProperties(lineItem, addedLineItem);
            }

            _orderRepository.Save(cart);

            // TODO: Always returns success, if we get warnings, we need to show them
            return new CartActionResult() { Success = true, Message = messages };
        }

        private void AddPropertiesToLineItem(ILineItem cartItem, LineItem addedItem, EntryContentBase entryContent)
        {
            string imageUrl = null;
            // Populate line item with as much as we can find
            if (string.IsNullOrEmpty(addedItem.ImageUrl) == false)
            {
                imageUrl = addedItem.ImageUrl;
            }
            else
            {
                imageUrl = entryContent.GetDefaultImage();
            }

            cartItem.Properties["ImageUrl"] = imageUrl;

            //if (string.IsNullOrEmpty(lineItem.ArticleNumber))
            //{
            //    lineItem.ArticleNumber = entry.ID;
            //}

            cartItem.DisplayName = entryContent.DisplayName;
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

        private void AddCustomProperties(LineItem lineItem, ILineItem cartItem)
        {

            // Make sure we have all available data on the item before
            // we proceed
            // lineItem.UpdateData(item);

            //TODO: Let specific model implementation populate these fields, we need to know too much about the model here
            cartItem.Properties[Constants.Metadata.LineItem.DisplayName] = lineItem.Name;
            cartItem.Properties[Constants.Metadata.LineItem.ImageUrl] = lineItem.ImageUrl;
            cartItem.Properties[Constants.Metadata.LineItem.Size] = lineItem.Size;
            cartItem.Properties[Constants.Metadata.LineItem.Description] = lineItem.Description;
            cartItem.Properties[Constants.Metadata.LineItem.Color] = lineItem.Color;
            cartItem.Properties[Constants.Metadata.LineItem.ColorImageUrl] = lineItem.ColorImageUrl;
            cartItem.Properties[Constants.Metadata.LineItem.ArticleNumber] = lineItem.ArticleNumber;
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
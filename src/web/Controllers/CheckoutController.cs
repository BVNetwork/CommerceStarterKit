/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using EPiServer.UI.Report;
using EPiServer.Web.Routing;
using Mediachase.BusinessFoundation.Data;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Website.Helpers;
using OxxCommerceStarterKit.Core;
using OxxCommerceStarterKit.Core.Customers;
using OxxCommerceStarterKit.Core.PaymentProviders.Payment;
using OxxCommerceStarterKit.Core.Repositories;
using OxxCommerceStarterKit.Core.Services;
using OxxCommerceStarterKit.Web.Business;
using OxxCommerceStarterKit.Web.Business.Analytics;
using OxxCommerceStarterKit.Web.Business.Payment;
using OxxCommerceStarterKit.Web.Models;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using OxxCommerceStarterKit.Web.Models.PageTypes.Payment;
using OxxCommerceStarterKit.Web.Models.PageTypes.System;
using OxxCommerceStarterKit.Web.Models.ViewModels;

namespace OxxCommerceStarterKit.Web.Controllers
{
    // TODO: Refactor the cart and checkout controller, should the base class be a Payment class? We should make this class smaller
    public class CheckoutController : PaymentBaseController<CheckoutPage>
    {
        private readonly CustomerAddressRepository _customerAddressRepository;
        private readonly ContactRepository _contactRepository;
        private readonly IContentRepository _contentRepository;
        private readonly ICurrentMarket _currentMarket;
        private readonly LocalizationService _localizationService;
        private readonly ICustomerFactory _customerFactory;
        private readonly IDefaultInventoryService _warehouseInventory;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly PaymentRegistry _paymentRegistry;

        public CheckoutController(CustomerAddressRepository customerAddressRepository, IContentRepository contentRepository, 
            ICurrentMarket currentMarket, ICustomerFactory customerFactory, 
            LocalizationService localizationService,
            IDefaultInventoryService warehouseInventory,
            IWarehouseRepository warehouseRepository,
            PaymentRegistry paymentRegistry)
        {
            _customerAddressRepository = customerAddressRepository;
            _contactRepository = new ContactRepository();
            _contentRepository = contentRepository;
            _currentMarket = currentMarket;
            _localizationService = localizationService;
            _customerFactory = customerFactory;
            _warehouseInventory = warehouseInventory;
            _warehouseRepository = warehouseRepository;
            _paymentRegistry = paymentRegistry;
        }

        public ActionResult Index(CheckoutPage currentPage)
        {
            CheckoutViewModel model = new CheckoutViewModel(currentPage);

            model.BillingAddress = _customerAddressRepository.GetDefaultBillingAddress();
            model.ShippingAddress = _customerAddressRepository.GetDefaultShippingAddress();
            var contact = _contactRepository.Get();
            model.Email = contact.Email;
            model.Phone = contact.PhoneNumber;

            model.PaymentSelection = GetPaymentInfo();
            model.AvailableCategories = GetAvailableCategories();

            model.TermsArticle = currentPage.TermsArticle;

            // Get cart and track it
            Api.CartController cartApiController = new Api.CartController();
            cartApiController.Language = currentPage.LanguageBranch;
            List<Core.Objects.LineItem> lineItems = cartApiController.GetItems(Cart.DefaultName);
            Track(lineItems);

            return View(model);
        }

        private void Track(List<Core.Objects.LineItem> lineItems)
        {
            // Track Analytics. 
            GoogleAnalyticsTracking tracking = new GoogleAnalyticsTracking(ControllerContext.HttpContext);

            // Add the products
            int i = 1;
            foreach (Core.Objects.LineItem lineItem in lineItems)
            {
                tracking.ProductAdd(code: lineItem.Code,
                    name: lineItem.Name,
                    quantity: (int)lineItem.Quantity,
                    price: (double)lineItem.PlacedPrice,
                    position: i
                    );
                i++;
            }

            // Step 2 is the checkout page
            tracking.Action("checkout", "{\"step\":2}");
        }


        private Dictionary<string, string> GetAvailableCategories()
        {
            var output = new Dictionary<string, string>();

            // get the Category type from the BusinessFoundation
            var category = DataContext.Current.GetMetaFieldType(Constants.Metadata.Customer.Category);
            if (category != null)
            {
                if (category.EnumItems != null)
                {
                    foreach (var item in category.EnumItems.OrderBy(x => x.OrderId))
                    {
                        output.Add(item.Handle.ToString(), item.Name);
                    }
                }
            }
            return output;
        }

        [HttpPost]
        public ActionResult Index(CheckoutPage currentPage, CheckoutViewModel model, PaymentSelection paymentSelection, int[] SelectedCategories)
        {
            model.PaymentSelection = paymentSelection;
            model.AvailableCategories = GetAvailableCategories();
            model.SelectedCategories = SelectedCategories;

            bool requireSSN = _paymentRegistry.PaymentMethodRequiresSocialSecurityNumber(paymentSelection);

            // validate input!
            ValidateFields(model, requireSSN);

            CartHelper ch = new CartHelper(Cart.DefaultName);

            // Verify that we actually have the items we're about to sell
            ConfirmStocks(ch.LineItems);

            if (ModelState.IsValid)
            {
                var billingAddress = model.BillingAddress.ToOrderAddress(Constants.Order.BillingAddressName);
                var shippingAddress = model.BillingAddress.ToOrderAddress(Constants.Order.ShippingAddressName);
                string username = model.Email.Trim();
                billingAddress.Email = username;
                billingAddress.DaytimePhoneNumber = model.Phone;

                HandleUserCreation(model, billingAddress, shippingAddress, username);

                if (ModelState.IsValid)
                {
                    // Checkout:

                    ch.Cart.OrderAddresses.Add(billingAddress);
                    ch.Cart.OrderAddresses.Add(shippingAddress);

                    AddShipping(ch.Cart, shippingAddress);

                    ch.Cart.OrderForms[0][Constants.Metadata.OrderForm.CustomerClub] = model.MemberClub;
                    if (model.SelectedCategories != null)
                    {
                        ch.Cart.OrderForms[0][Constants.Metadata.OrderForm.SelectedCategories] = string.Join(",", model.SelectedCategories);
                    }
                    OrderGroupWorkflowManager.RunWorkflow(ch.Cart, OrderGroupWorkflowManager.CartPrepareWorkflowName);

                    AddPayment(ch.Cart, paymentSelection.SelectedPayment.ToString(), billingAddress);

                    ch.Cart.AcceptChanges();

                    var url = _paymentRegistry.GetPaymentMethodPageUrl(model.PaymentSelection.SelectedPayment.ToString());

                    if (url != null)
                    {
                        return Redirect(url);
                    }
                }
            }

            Guid? selectedPayment = model.PaymentSelection.SelectedPayment;
            model.PaymentSelection = GetPaymentInfo();
            if (selectedPayment.HasValue)
            {
                model.PaymentSelection.SelectedPayment = selectedPayment.Value;
            }
            model.TermsArticle = currentPage.TermsArticle;

            return View(model);
        }

        /// <summary>
        /// Perform an additional stock check
        /// </summary>
        /// <param name="lineItems"></param>
        void ConfirmStocks(IEnumerable<LineItem> lineItems)
        {
            // TODO: Additonal stock check, if necessary - here is your chance to update stock from the ERP system for an example

            // TODO: Confirm use of default warehouse for your project
            var epiWarehouses = _warehouseRepository.List().Where(w => w.Code.Equals("default")).ToList();

            if (lineItems == null || lineItems.Any() == false)
                return;

            // [sku] = quantity, displayname
            var summary = new Dictionary<string, Tuple<decimal, string>>();

            foreach (var i in lineItems)
            {
                Tuple<decimal, string> quantity;

                if (summary.TryGetValue(i.Code, out quantity))
                {
                    quantity = new Tuple<decimal, string>(quantity.Item1 + i.Quantity, quantity.Item2);
                }
                else
                {
                    quantity = new Tuple<decimal, string>(i.Quantity, i.DisplayName);
                }

                summary[i.Code] = quantity;
            }

            foreach (var p in summary)
            {
                var catalogKey = new CatalogKey(Mediachase.Commerce.Core.AppContext.Current.ApplicationId, p.Key);
                var inventory = _warehouseInventory.GetForDefaultWarehouse(p.Key);

                if (inventory == null || inventory.PurchaseAvailableQuantity < p.Value.Item1)
                {
                    /// TODO: Add language string
                    ModelState.AddModelError(string.Empty, "Variant is out of stock " + p.Value.Item2 + " (" + p.Key + ")");
                }
            }
        }

        private void HandleUserCreation(CheckoutViewModel model, OrderAddress billingAddress, OrderAddress shippingAddress, string username)
        {
            // has specified password
            if (!User.Identity.IsAuthenticated && !string.IsNullOrEmpty(model.Password))
            {
                // check if email exists
                var user = Membership.GetUser(username);
                if (user != null)
                {
                    // user exists
                    if (Membership.ValidateUser(username, model.Password))
                    {
                        // cannot login here because something strange happens with the order
                        //LoginController.CreateAuthenticationCookie(ControllerContext.HttpContext, username, AppContext.Current.ApplicationName, false);
                    }
                    else
                    {
                        ModelState.AddModelError("Password", _localizationService.GetString("/common/account/login_error"));
                    }
                }
                else
                {
                    // create new customer
                    var customer = _customerFactory.CreateCustomer(username, model.Password, model.Phone, billingAddress, shippingAddress, true, createStatus =>
                    {
                        switch (createStatus)
                        {
                            case MembershipCreateStatus.DuplicateUserName:
                                ModelState.AddModelError("Password", _localizationService.GetString("/common/account/register_error_unique_username"));
                                break;
                            case MembershipCreateStatus.InvalidPassword:
                                ModelState.AddModelError("Password", _localizationService.GetString("/common/account/register_error"));
                                break;
                            default:
                                ModelState.AddModelError("Password", createStatus.ToString());
                                break;
                        }
                    });
                    if (customer != null)
                    {
                        // customer was created
                        // cannot login here because something strange happens with the order
                        // TODO: Investigate this
                        //LoginController.CreateAuthenticationCookie(ControllerContext.HttpContext, username, AppContext.Current.ApplicationName, false);
                    }
                }
            }
        }

        private void ValidateFields(CheckoutViewModel model, bool requireSSN)
        {
            string requiredString = _localizationService.GetString("/common/validation/required");
            if (string.IsNullOrEmpty(model.BillingAddress.FirstName))
            {
                ModelState.AddModelError("BillingAddress.FirstName", string.Format(requiredString, _localizationService.GetString("/common/accountpages/firstname_label")));
            }
            if (string.IsNullOrEmpty(model.BillingAddress.LastName))
            {
                ModelState.AddModelError("BillingAddress.LastName", string.Format(requiredString, _localizationService.GetString("/common/accountpages/lastname_label")));
            }
            if (string.IsNullOrEmpty(model.BillingAddress.StreetAddress))
            {
                ModelState.AddModelError("BillingAddress.StreetAddress", string.Format(requiredString, _localizationService.GetString("/common/accountpages/address_label")));
            }
            if (string.IsNullOrEmpty(model.BillingAddress.ZipCode))
            {
                ModelState.AddModelError("BillingAddress.Zip", string.Format(requiredString, _localizationService.GetString("/common/accountpages/zipcode_label")));
            }
            if (string.IsNullOrEmpty(model.BillingAddress.City))
            {
                ModelState.AddModelError("BillingAddress.PostalCode", string.Format(requiredString, _localizationService.GetString("/common/accountpages/city_label")));
            }
            //if (string.IsNullOrEmpty(model.ShippingAddress.FirstName))
            //{
            //    ModelState.AddModelError("ShippingAddress.FirstName", string.Format(requiredString, _localizationService.GetString("/common/accountpages/firstname_label")));
            //}
            //if (string.IsNullOrEmpty(model.ShippingAddress.LastName))
            //{
            //    ModelState.AddModelError("ShippingAddress.LastName", string.Format(requiredString, _localizationService.GetString("/common/accountpages/lastname_label")));
            //}
            //if (string.IsNullOrEmpty(model.ShippingAddress.StreetAddress))
            //{
            //    ModelState.AddModelError("ShippingAddress.StreetAddress", string.Format(requiredString, _localizationService.GetString("/common/accountpages/address_label")));
            //}
            //if (string.IsNullOrEmpty(model.ShippingAddress.ZipCode))
            //{
            //    ModelState.AddModelError("ShippingAddress.Zip", string.Format(requiredString, _localizationService.GetString("/common/accountpages/zipcode_label")));
            //}
            //if (string.IsNullOrEmpty(model.ShippingAddress.City))
            //{
            //    ModelState.AddModelError("ShippingAddress.PostalCode", string.Format(requiredString, _localizationService.GetString("/common/accountpages/city_label")));
            //}

            /*if (string.IsNullOrEmpty(model.ShippingAddress.DeliveryServicePoint))
            {
                ModelState.AddModelError("ShippingAddress.DeliveryServicePoint", _localizationService.GetString("/common/accountpages/missing_delivery_service_point"));
            }*/

            if (string.IsNullOrEmpty(model.Email))
            {
                ModelState.AddModelError("Email", string.Format(requiredString, _localizationService.GetString("/common/accountpages/email_label").Replace(":", "")));
            }
            if (string.IsNullOrEmpty(model.Phone))
            {
                ModelState.AddModelError("Phone", string.Format(requiredString, _localizationService.GetString("/common/accountpages/phone_label").Replace(":", "")));
            }

            if (requireSSN)
            {
                if (string.IsNullOrEmpty(model.SocialSecurityNumber))
                {
                    ModelState.AddModelError("SocialSecurityNumber", string.Format(requiredString, _localizationService.GetString("/common/checkout/ssn_label").Replace(":", "")));
                }
            }

            if (!string.IsNullOrEmpty(model.Password))
            {
                //ModelState.AddModelError("Password", string.Format(requiredString, _localizationService.GetString("/common/checkout/placeholder/password")));

                if (string.IsNullOrEmpty(model.ConfirmPassword))
                {
                    ModelState.AddModelError("ConfirmPassword", string.Format(requiredString, _localizationService.GetString("/common/checkout/placeholder/confirmpassword")));
                }
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", string.Format(_localizationService.GetString("/common/validation/compare"),
                        _localizationService.GetString("/common/checkout/placeholder/password"),
                        _localizationService.GetString("/common/checkout/placeholder/confirmpassword")));
                }
            }

            if (model.PaymentSelection.SelectedPayment == Guid.Empty)
            {
                ModelState.AddModelError("PaymentInfo.SelectedPayment", string.Format(requiredString,
                        _localizationService.GetString("/common/checkout/payment_methods_title")));
            }


            if (!model.AcceptedTerms)
            {
                ModelState.AddModelError("AcceptedTerms", _localizationService.GetString("/common/checkout/terms_error"));
            }
        }

        private PaymentSelection GetPaymentInfo()
        {

            var paymentMethods = PaymentManager.GetPaymentMethods(_currentMarket.GetCurrentMarket().DefaultLanguage.ToString());
            var paymentInfo = new PaymentSelection();

            foreach (var paymentMethodRow in paymentMethods.PaymentMethod.Where(x=>x.SystemKeyword != "quickbuy").OrderBy(p => p.Ordering))
            {
                var methodInfo = new PaymentMethodInfo();
                methodInfo.PaymentMethod = paymentMethodRow;

                // Enrich with payment information from page
                var page = _paymentRegistry.GetPaymentContentPageByMethodId(paymentMethodRow.PaymentMethodId);
                methodInfo.Description = page.Description != null ? page.Description.ToHtmlString() : string.Empty;
                paymentInfo.PaymentMethods.Add(methodInfo);
            }

            // if there is only 1 choice, select it as default
            if (paymentInfo.PaymentMethods.Count == 1)
            {
                paymentInfo.PaymentMethods.First().PaymentMethod.IsDefault = true;
            }
            return paymentInfo;
        }

        private void AddShipping(Cart c, OrderAddress shippingAddress)
        {
            foreach (Shipment ship in c.OrderForms[0].Shipments)
            {
                ship.Delete();
            }

            Shipment shipment = c.OrderForms[0].Shipments.AddNew();
            shipment.ShippingAddressId = shippingAddress.Name;

            for (int i = 0; i < c.OrderForms[0].LineItems.Count; i++)
            {
                LineItem item = c.OrderForms[0].LineItems[i];
                shipment.AddLineItemIndex(i, item.Quantity);
            }

            if (c.OrderForms[0].Shipments != null && c.OrderForms[0].Shipments.Count > 0)
            {
                shipment = c.OrderForms[0].Shipments[0];
                ShippingMethodAndRate selectedShippingMethod = GetSelectedShippingMethod();

                if (selectedShippingMethod != null)
                {
                    shipment.ShippingMethodId = selectedShippingMethod.ShippingMethodId;
                    shipment.ShippingMethodName = selectedShippingMethod.ShippingMethodName;
                    c.AcceptChanges();
                }
            }

            c.AcceptChanges();
        }

        private ShippingMethodAndRate GetSelectedShippingMethod()
        {
            ShippingMethodAndRate selectedShippingMethod = null;

            var shippingMethods = ShippingManager.GetShippingMethodsByMarket(_currentMarket.GetCurrentMarket().MarketId.Value, false);

            // TODO: Make this configurable, as we do not specify
            // shipping during checkout in
            var selectedShipping = shippingMethods.ShippingMethod.FirstOrDefault(m => m.IsActive && m.IsDefault);
            if (selectedShipping == null)
            {
                throw new ApplicationException("No active and default shipping method for current market (" + _currentMarket.GetCurrentMarket().MarketId.Value + ")");
            }

            Guid methodId = selectedShipping.ShippingMethodId;
            string methodName = selectedShipping.DisplayName;

            decimal rate = 100m;

            selectedShippingMethod = new ShippingMethodAndRate(methodName, string.Empty, rate, methodId);

            return selectedShippingMethod;
        }

        private IEnumerable<ShippingMethodAndRate> GetShippingMethodsWithRates(Cart cart, OrderAddress address)
        {
            ShippingMethodDto shippingMethods = ShippingManager.GetShippingMethods(CurrentPage.LanguageID, false);
            List<ShippingMethodAndRate> shippingMethodsList = new List<ShippingMethodAndRate>();
            string outputMessage = string.Empty;
            string shippingMethodAndPrice = string.Empty;

            //get the one shipment in the order
            Shipment ship = null;
            if (cart.OrderForms[0].Shipments != null && cart.OrderForms[0].Shipments.Count > 0)
            {
                ship = cart.OrderForms[0].Shipments[0];
            }

            if (ship != null)
            {
                // request rates, make sure we request rates not bound to selected delivery method
                foreach (ShippingMethodDto.ShippingMethodRow row in shippingMethods.ShippingMethod)
                {
                    shippingMethodsList.Add(GetShippingRateInfo(row, ship));
                }
            }

            return shippingMethodsList;
        }

        private ShippingMethodAndRate GetShippingRateInfo(ShippingMethodDto.ShippingMethodRow row, Shipment shipment)
        {
            ShippingMethodAndRate returnRate = null;
            string nameAndRate = string.Empty;

            // Check if package contains shippable items, if it does not use the default shipping method instead of the one specified
            Type type = Type.GetType(row.ShippingOptionRow.ClassName);
            if (type == null)
            {
                throw new TypeInitializationException(row.ShippingOptionRow.ClassName, null);
            }

            string outputMessage = string.Empty;
            IShippingGateway provider = (IShippingGateway)Activator.CreateInstance(type);

            if (shipment != null)
            {
                ShippingRate rate = provider.GetRate(_currentMarket.GetCurrentMarket(), row.ShippingMethodId, shipment, ref outputMessage);
                nameAndRate = string.Format("{0} : {1}", row.Name, rate.Money.Amount.ToString("C"));
                returnRate = new ShippingMethodAndRate(row.Name, nameAndRate, rate.Money.Amount, row.ShippingMethodId);
            }

            return returnRate;
        }

        private void AddPayment(Cart cart, string paymentMethod, OrderAddress billingAddress)
        {
            //first delete existing payment
            if (cart.OrderForms[0].Payments != null && cart.OrderForms[0].Payments.Count > 0)
            {
                foreach (Payment pay in cart.OrderForms[0].Payments)
                {
                    pay.Delete();
                }
            }

            PaymentMethodDto method = PaymentManager.GetPaymentMethod(new Guid(paymentMethod));

            DibsPayment newPayment = new DibsPayment();
            newPayment.PaymentType = PaymentType.Other;
            newPayment.PaymentMethodId = method.PaymentMethod[0].PaymentMethodId;
            newPayment.PaymentMethodName = method.PaymentMethod[0].Name;
            newPayment.Amount = cart.Total;

            if (billingAddress != null)
            {
                newPayment.BillingAddressId = billingAddress.Name;
            }

            cart.OrderForms[0].Payments.Add(newPayment);

        }

    }
}

/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web.Security;
using AjaxControlToolkit;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Newtonsoft.Json;
using OxxCommerceStarterKit.Core.Customers;
using OxxCommerceStarterKit.Core.Email;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Core.Objects;
using OxxCommerceStarterKit.Core.Repositories.Interfaces;
using OxxCommerceStarterKit.Core.Objects.SharedViewModels;
using EPiServer.Logging;

namespace OxxCommerceStarterKit.Core.Services
{
    public class OrderService : IOrderService
    {
        private static readonly ILogger Log = LogManager.GetLogger();
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerFactory _customerFactory;
        private readonly IEmailService _emailService;
        private readonly IOrderSettings _orderSettings;

        public OrderService(IOrderRepository orderRepository, ICustomerFactory customerFactory, IEmailService emailService, IOrderSettings orderSettings)
        {
            _orderRepository = orderRepository;
            _customerFactory = customerFactory;
            _emailService = emailService;
            _orderSettings = orderSettings;
        }

        public PurchaseOrderModel GetOrderByTrackingNumber(string trackingNumber)
        {
            return MapToModel(_orderRepository.GetOrderByTrackingNumber(trackingNumber));
        }

        public IEnumerable<PurchaseOrderModel> GetOrdersByUserId(Guid customerId)
        {
            var orders = _orderRepository.GetOrdersByUserId(customerId);
            return orders == null ? Enumerable.Empty<PurchaseOrderModel>() : orders.Select(MapToModel).ToList();
        }

        private PurchaseOrderModel MapToModel(PurchaseOrder purchaseOrder)
        {
            if (purchaseOrder == null)
                return null;
            return new PurchaseOrderModel()
            {
                BackendOrderNumber = purchaseOrder.GetStringValue(Constants.Metadata.PurchaseOrder.BackendOrderNumber),
                Created = purchaseOrder.Created,
                OrderForms = purchaseOrder.OrderForms.OrEmpty().Select(MapOrderForm),
                OrderAddresses = purchaseOrder.OrderAddresses.OrEmpty().Select(MapOrderAddress),
                ShippingTotal = purchaseOrder.ShippingTotal,
                Status = purchaseOrder.Status,
                TaxTotal = purchaseOrder.TaxTotal,
                Total = purchaseOrder.Total,
                TrackingNumber = purchaseOrder.TrackingNumber,
                BillingEmail = GetBillingEmail(purchaseOrder),
                BillingPhone = GetBillingPhone(purchaseOrder),
                ProviderId = purchaseOrder.ProviderId,
                MarketId = purchaseOrder.MarketId
            };
        }

        private OrderFormModel MapOrderForm(OrderForm orderForm)
        {
            return new OrderFormModel()
            {
                Discounts = MapDiscounts(orderForm.Discounts),
                LineItems = GetLineItems(orderForm),
                Payments = orderForm.Payments.OrEmpty().Select(MapToModel).ToArray(),
                Shipments = orderForm.Shipments.OrEmpty().Select(MapToModel).ToArray()
            };
        }

        private ShipmentModel MapToModel(Shipment shipment)
        {
            return new ShipmentModel()
            {
                Discounts = MapDiscounts(shipment.Discounts),
                ShipmentTrackingNumber = shipment.ShipmentTrackingNumber,
                ShippingDiscountAmount = shipment.ShippingDiscountAmount
            };
        }

        private DiscountModel[] MapDiscounts(ShipmentDiscountCollection discounts)
        {
            if (discounts == null || discounts.Count == 0)
                return new DiscountModel[0];
            var models = new List<DiscountModel>(discounts.Count);
            for (var ii = 0; ii < discounts.Count; ii++)
            {
                models.Add(new DiscountModel()
                {
                    DiscountCode = discounts[ii].DiscountCode
                });
            }
            return models.ToArray();
        }

        private PaymentModel MapToModel(Payment payment)
        {
            return new PaymentModel()
            {
                PaymentMethodName = payment.PaymentMethodName
            };
        }

        private static DiscountModel[] MapDiscounts(OrderFormDiscountCollection discounts)
        {
            if (discounts == null || discounts.Count == 0)
                return new DiscountModel[0];
            var models = new List<DiscountModel>(discounts.Count);
            for (var ii = 0; ii < discounts.Count; ii++)
            {
                models.Add(new DiscountModel()
                {
                    DiscountCode = discounts[ii].DiscountCode
                });
            }
            return models.ToArray();
        }

        private IEnumerable<LineItemModel> GetLineItems(OrderForm orderForm)
        {
            /// TODO: Extract warehouse code from shipment
            return orderForm.LineItems.Select(MapToModel);
        }

        private LineItemModel MapToModel(Mediachase.Commerce.Orders.LineItem item)
        {
            return new LineItemModel()
            {
                ArticleNumber = item.GetStringValue(Constants.Metadata.LineItem.ArticleNumber),
                CatalogEntryId = item.Code,
                Color = item.GetStringValue(Constants.Metadata.LineItem.Color),
                Description = item.GetStringValue(Constants.Metadata.LineItem.Description),
                Discounts = MapDiscounts(item.Discounts),
                DisplayName = item.DisplayName,
                ExtendedPrice = item.ExtendedPrice,
                LineItemDiscountAmount = item.LineItemDiscountAmount,
                OrderLevelDiscountAmount = item.OrderLevelDiscountAmount,
                Quantity = (int)item.Quantity,
                Size = item.GetStringValue(Constants.Metadata.LineItem.Size),
                WarehouseCode = item.WarehouseCode, 
                ImageUrl = item.GetStringValue(Constants.Metadata.LineItem.ImageUrl)
            };
        }

        private DiscountModel[] MapDiscounts(LineItemDiscountCollection discounts)
        {
            if (discounts == null || discounts.Count == 0)
                return new DiscountModel[0];
            var models = new List<DiscountModel>(discounts.Count);
            for (var ii = 0; ii < discounts.Count; ii++)
            {
                models.Add(new DiscountModel()
                {
                    DiscountCode = discounts[ii].DiscountCode
                });
            }
            return models.ToArray();
        }

        private OrderAddressModel MapOrderAddress(OrderAddress address)
        {
            return new OrderAddressModel()
            {
                City = address.City,
                CountryCode = address.CountryCode,
                DeliveryServicePoint = GetDeliveryServicePointFrom(address),
                FirstName = address.FirstName,
                LastName = address.LastName,
                Id = address.Id,
                Line1 = address.Line1,
                Name = address.Name,
                PostalCode = address.PostalCode
            };
        }

        private string GetDeliveryServicePointFrom(OrderAddress shippingAddress)
        {
            if (string.IsNullOrWhiteSpace((string)shippingAddress[Constants.Metadata.Address.DeliveryServicePoint]))
                return string.Empty;
            try
            {
                var deliveryServicePoint =
                    JsonConvert.DeserializeObject<ServicePoint>(
                        (string)shippingAddress[Constants.Metadata.Address.DeliveryServicePoint]);
                return deliveryServicePoint.Name;
            }
            catch (Exception ex)
            {
                // Todo: Move to method with more documentation about why this can fail
                Log.Error("Error during deserializing delivery location", ex);
            }
            return string.Empty;
        }

        private string GetBillingEmail(PurchaseOrder purchaseOrder)
        {
            try
            {
                return purchaseOrder.GetBillingEmail();
            }
            catch (Exception ex)
            {
                // TODO: Inspect this, do we need a try catch here?
                Log.Error("Error getting email for customer", ex);
            }
            return string.Empty;
        }

        private string GetBillingPhone(PurchaseOrder purchaseOrder)
        {
            try
            {
                return purchaseOrder.GetBillingPhone();
            }
            catch (Exception ex)
            {
                // TODO: Inspect this, do we need a try catch here?
                Log.Error("Error getting phone for customer", ex);
            }
            return string.Empty;
        }

        public void FinalizeOrder(string trackingNumber, IIdentity identity)
        {
            var order = _orderRepository.GetOrderByTrackingNumber(trackingNumber);
            // Create customer if anonymous
            CreateUpdateCustomer(order, identity);

            var shipment = order.OrderForms.First().Shipments.First();

            // 
            if(_orderSettings.ReleaseShipmentAutomatically)
            {
                OrderStatusManager.ReleaseOrderShipment(shipment);
                OrderStatusManager.PickForPackingOrderShipment(shipment);
            }
            order.AcceptChanges();
        }

        public void CreateUpdateCustomer(PurchaseOrder order, IIdentity identity)
        {
            // try catch so this does not interrupt the order process.
            try
            {
                var billingAddress = order.OrderAddresses.FirstOrDefault(x => x.Name == Constants.Order.BillingAddressName);
                var shippingAddress = order.OrderAddresses.FirstOrDefault(x => x.Name == Constants.Order.ShippingAddressName);

                // create customer if anonymous, or update join customer club and selected values on existing user
                MembershipUser user = null;
                if (!identity.IsAuthenticated)
                {
                    string email = null;
                    if (billingAddress != null)
                    {
                        email = billingAddress.Email.Trim();
                        user = Membership.GetUser(email);
                    }

                    if (user == null)
                    {
                        var customer = CreateCustomer(email, Guid.NewGuid().ToString(), 
                            billingAddress.DaytimePhoneNumber, billingAddress, shippingAddress, false, 
                            createStatus => Log.Error("Failed to create user during order completion. " + createStatus.ToString()));
                        if (customer != null)
                        {
                            order.CustomerId = Guid.Parse(customer.PrimaryKeyId.Value.ToString());
                            order.CustomerName = customer.FirstName + " " + customer.LastName;
                            order.AcceptChanges();

                            SetExtraCustomerProperties(order, customer);

                            _emailService.SendWelcomeEmail(billingAddress.Email);
                        }
                    }
                    else
                    {
                        var customer = CustomerContext.Current.GetContactForUser(user);
                        order.CustomerName = customer.FirstName + " " + customer.LastName;
                        order.CustomerId = Guid.Parse(customer.PrimaryKeyId.Value.ToString());
                        order.AcceptChanges();
                        SetExtraCustomerProperties(order, customer);

                    }
                }
                else
                {
                    user = Membership.GetUser(identity.Name);
                    var customer = CustomerContext.Current.GetContactForUser(user);
                    SetExtraCustomerProperties(order, customer);
                }
            }
            catch (Exception ex)
            {
                // Log here
                Log.Error("Error during creating / update user", ex);
            }
        }

        /// <summary>
        /// If customer has joined the members club, then add the interest areas to the
        /// customer profile.
        /// </summary>
        /// <remarks>
        /// The request to join the member club is stored on the order during checkout.
        /// </remarks>
        /// <param name="order">The order.</param>
        /// <param name="customer">The customer.</param>
        private void SetExtraCustomerProperties(PurchaseOrder order, CustomerContact customer)
        {
            if (UserHasRegisteredForMembersClub(order))
                UpdateCustomerWithMemberClubInfo(order, customer);
        }

        private static bool UserHasRegisteredForMembersClub(PurchaseOrder order)
        {
            return order.OrderForms[0][Constants.Metadata.OrderForm.CustomerClub] != null && ((bool)order.OrderForms[0][Constants.Metadata.OrderForm.CustomerClub]);
        }

        private static void UpdateCustomerWithMemberClubInfo(PurchaseOrder order, CustomerContact customer)
        {
            customer.CustomerGroup = Constants.CustomerGroup.CustomerClub;
            customer.SetCategories(GetSelectedInterestCategoriesFrom(order));
            customer.SaveChanges();
        }

        private static int[] GetSelectedInterestCategoriesFrom(PurchaseOrder order)
        {
            var selectedCategories =
                (order.OrderForms[0][Constants.Metadata.OrderForm.SelectedCategories] as string ?? "").Split(',').Select(x =>
                {
                    int i = 0;
                    Int32.TryParse(x, out i);
                    return i;
                }).Where(x => x > 0).ToArray();
            return selectedCategories;
        }

        protected CustomerContact CreateCustomer(string email, string password, string phone, OrderAddress billingAddress, OrderAddress shippingAddress, bool hasPassword, Action<MembershipCreateStatus> userCreationFailed)
        {
            return _customerFactory.CreateCustomer(email, password, phone, billingAddress, shippingAddress, hasPassword,
                userCreationFailed);
        }

        public bool SendOrderReceipt(string trackingNumber)
        {
            PurchaseOrderModel model = GetOrderByTrackingNumber(trackingNumber);
            return SendOrderReceipt(model);
        }

        public bool SendOrderReceipt(PurchaseOrder order)
        {
            return SendOrderReceipt(MapToModel(order));
        }

        public bool SendOrderReceipt(PurchaseOrderModel orderModel)
        {
            IEmailService emailService = EPiServer.ServiceLocation.ServiceLocator.Current.GetInstance<IEmailService>();
            if(orderModel == null)
                throw new ArgumentNullException("orderModel");
            return emailService.SendOrderReceipt(orderModel);
        }

    }
}

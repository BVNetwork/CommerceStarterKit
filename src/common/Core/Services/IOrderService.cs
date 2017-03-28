/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Security.Principal;
using OxxCommerceStarterKit.Core.Models;
using OxxCommerceStarterKit.Core.Objects.SharedViewModels;

namespace OxxCommerceStarterKit.Core.Services
{
    public interface IOrderService
    {
        PurchaseOrderModel GetOrderByTrackingNumber(string trackingNumber);
        IEnumerable<PurchaseOrderModel> GetOrdersByUserId(Guid customerId);
        void FinalizeOrder(string orderTrackingNumber, IIdentity identity);
        bool SendOrderReceipt(string trackingNumber);
        PurchaseOrderModel QuickBuyOrder(QuickBuyModel model, Guid customerId);
    }
}

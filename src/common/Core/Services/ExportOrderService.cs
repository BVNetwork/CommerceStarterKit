/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using Mediachase.Commerce.Orders;
using OxxCommerceStarterKit.Interfaces;

namespace OxxCommerceStarterKit.Core.Services
{
    /// <summary>
    /// Exports orders to a backend system (typically an ERP system)
    /// </summary>
	public class ExportOrderService : IExportOrderService
	{
		public string ExportOrder(PurchaseOrder purchaseOrder)
		{
            /// TODO: Implement in project
		    throw new NotImplementedException("This service requires custom implementation.");
		}
	}
}

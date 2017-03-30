/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Linq;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Search;
using OxxCommerceStarterKit.Core.Repositories.Interfaces;

namespace OxxCommerceStarterKit.Core.Repositories
{
    public class OrderRepository
    {
        public PurchaseOrder GetOrderById(int orderGroupId)
        {
            return OrderContext.Current.GetPurchaseOrderById(orderGroupId);
        }

  

		public List<PurchaseOrder> GetOrdersByUserId(Guid customerId)
		{
			return OrderContext.Current.GetPurchaseOrders(customerId).ToList();
		}

        private List<PurchaseOrder> GetOrders(string sqlWhereClause)
        {
            return GetOrders(sqlWhereClause, string.Empty);
        }

        private List<PurchaseOrder> GetOrdersByMetaField(string sqlMetaWhereClause, int recordCount = int.MaxValue)
        {
            return GetOrders(string.Empty, sqlMetaWhereClause, recordCount);
        }

        private List<PurchaseOrder> GetOrders(string sqlWhereClause, string sqlMetaWhereClause, int recordCount = int.MaxValue)
        {
            var orderSearchParameters = new OrderSearchParameters();
            if (!string.IsNullOrEmpty(sqlWhereClause))
            {
                orderSearchParameters.SqlWhereClause = sqlWhereClause;
            }

            if (!string.IsNullOrEmpty(sqlMetaWhereClause))
            {
                orderSearchParameters.SqlMetaWhereClause = sqlMetaWhereClause;
            }

            var orderSearchOptions = new OrderSearchOptions();
            orderSearchOptions.Namespace = "Mediachase.Commerce.Orders";
            orderSearchOptions.Classes.Add("PurchaseOrder");
            orderSearchOptions.Classes.Add("Shipment");
            orderSearchOptions.CacheResults = false;
            orderSearchOptions.RecordsToRetrieve = recordCount;

            return OrderContext.Current.FindPurchaseOrders(orderSearchParameters, orderSearchOptions).ToList();
        }
    }
}

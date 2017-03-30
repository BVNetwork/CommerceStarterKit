using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Search;

namespace OxxCommerceStarterKit.Core.Extensions
{
    public static class OrderRepositoryExtensions
    {
        public static PurchaseOrder GetOrderByTrackingNumber(this IOrderRepository repo, string orderNumber)
        {
            string sqlMetaWhereClause = string.Format(@"META.TrackingNumber = '{0}'",
               orderNumber);

            var purchaseOrders = GetOrdersByMetaField(sqlMetaWhereClause, 1);

            int orderIdNumeric = 0;
            if (purchaseOrders == null || purchaseOrders.Count == 0 || int.TryParse(orderNumber, out orderIdNumeric))
            {
                return OrderContext.Current.GetPurchaseOrderById(orderIdNumeric);
            }

            if (purchaseOrders != null && purchaseOrders.Count > 0)
            {
                return purchaseOrders.FirstOrDefault();
            }

            return null;
        }

        private static List<PurchaseOrder> GetOrdersByMetaField(string sqlMetaWhereClause, int recordCount = int.MaxValue)
        {
            return GetOrders(string.Empty, sqlMetaWhereClause, recordCount);
        }

        private static List<PurchaseOrder> GetOrders(string sqlWhereClause, string sqlMetaWhereClause, int recordCount = int.MaxValue)
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

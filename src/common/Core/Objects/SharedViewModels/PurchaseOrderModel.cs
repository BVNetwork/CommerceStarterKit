using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OxxCommerceStarterKit.Core.Objects.SharedViewModels
{
    public class PurchaseOrderModel
    {
        public string TrackingNumber { get; set; }

        public DateTime Created { get; set; }

        public IEnumerable<OrderFormModel> OrderForms { get; set; }

        public string Status { get; set; }

        public decimal TaxTotal { get; set; }

        public decimal Total { get; set; }

        public decimal ShippingTotal { get; set; }

        public string BillingEmail { get; set; }

        public string BillingPhone { get; set; }

        public IEnumerable<OrderAddressModel> OrderAddresses { get; set; }

        public string BackendOrderNumber { get; set; }

        public string ProviderId { get; set; }

        public Mediachase.Commerce.MarketId MarketId { get; set; }
        public string Frequency { get; set; }
        public DateTime? LatestDelivery { get; set; }
    }
}

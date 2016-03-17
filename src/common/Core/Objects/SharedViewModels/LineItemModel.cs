using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OxxCommerceStarterKit.Core.Objects.SharedViewModels
{
    public class LineItemModel
    {
        public decimal ExtendedPrice { get; set; }

        public decimal LineItemDiscountAmount { get; set; }

        public decimal OrderLevelDiscountAmount { get; set; }

        public string CatalogEntryId { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public int Quantity { get; set; }

        public string Size { get; set; }

        public string Color { get; set; }

        public string ArticleNumber { get; set; }

        public DiscountModel[] Discounts { get; set; }

        public string WarehouseCode { get; set; }
        public string ImageUrl { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OxxCommerceStarterKit.Web.Models.ViewModels
{
    public class QuickBuyViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string PhoneNumber { get; set; }
        public string Mail { get; set; }
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string Sku { get; set; }

        public IEnumerable<ProductInfo> Products { get; set; }

        public string SelectedSku { get; set; }
    }

    public class ProductInfo
    {
        public string Name { get; set; }
        public string Sku { get; set; }
    }
}
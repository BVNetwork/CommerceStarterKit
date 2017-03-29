using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxxCommerceStarterKit.Core.Models
{
    public class QuickBuyModel
    {
        public Guid CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string PhoneNumber { get; set; }
        public string Mail { get; set; }
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string Sku { get; set; }

        public Frequency Frequency { get; set; }


    }

    public enum Frequency
    {
        Week,
        Month,
        Quarter,
        HalfYear
    }
}

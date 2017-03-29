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

        public string CouponCode { get; set; }

        public Frequency Frequency { get; set; }

        public override string ToString()
        {
            return string.Format("{0}#{1}#{2}#{3}#{4}#{5}#{6}", FirstName, LastName, Address, ZipCode, City,
                PhoneNumber, Mail);
        }

        public static QuickBuyModel FromString(string value)
        {
            var values = value.Split('#');

            if (values.Length != 7)
            {
                return new QuickBuyModel();
            }

            QuickBuyModel model = new QuickBuyModel();
            model.FirstName = values[0];
            model.LastName = values[1];
            model.Address = values[2];
            model.ZipCode = values[3];
            model.City = values[4];
            model.PhoneNumber = values[5];
            model.Mail = values[6];
            return model;
        }
    }

    public enum Frequency
    {
        Week,
        Month,
        Quarter,
        HalfYear
    }
}

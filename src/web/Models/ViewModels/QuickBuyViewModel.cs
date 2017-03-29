using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.Validation.Internal;
using OxxCommerceStarterKit.Core.Models;
using OxxCommerceStarterKit.Web.Models.Blocks;
using OxxCommerceStarterKit.Web.Models.Files;

namespace OxxCommerceStarterKit.Web.Models.ViewModels
{
    public class QuickBuyViewModel: QuickBuyModel
    {
        public QuickBuyViewModel()
        {
            Frequencies = Enum.GetValues(typeof(Frequency)).Cast<Frequency>().ToList();
        }

        public QuickBuyViewModel(QuickBuyModel cookieModel)
        {
            Frequencies = Enum.GetValues(typeof(Frequency)).Cast<Frequency>().ToList();
            FirstName = cookieModel.FirstName;
            LastName = cookieModel.LastName;
            Address = cookieModel.Address;
            ZipCode = cookieModel.ZipCode;
            City = cookieModel.ZipCode;
            PhoneNumber = cookieModel.PhoneNumber;
            Mail = cookieModel.Mail;
        }

        public QuickBuyBlock CurrentBlock { get; set; }

        public bool ApproveDisclaimer { get; set; }

        public IEnumerable<ProductInfo> Products { get; set; }

        public IEnumerable<Frequency> Frequencies { get; set; }

        public string SelectedSku { get; set; }
        public bool HasImage { get; set; }
        public ImageViewModel ImageContent { get; set; }
        public string Language { get; set; }
        public bool Success { get; set; }
        public string OrderNumber { get; set; }        
    }

    public class ProductInfo
    {
        public string Name { get; set; }
        public string Sku { get; set; }
    }
}
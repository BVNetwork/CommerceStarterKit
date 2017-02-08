using System.Collections.Generic;
using EPiServer.Commerce.Catalog.ContentTypes;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using OxxCommerceStarterKit.Web.Models.ViewModels.Email;

namespace OxxCommerceStarterKit.Web.Models.ViewModels
{
    public class NewsletterWithProductsViewModel : NewsletterViewModel
    {
        public NewsletterWithProductsViewModel(NewsletterPage currentPage, NotificationSettings settings) : base(currentPage, settings)
        {
            Products = new List<EntryContentBase>();
        }

        public IEnumerable<EntryContentBase> Products;
        public string ProductListTitle;
        

    }
}
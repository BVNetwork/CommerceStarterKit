using System.Collections.Generic;
using System.Linq;
using OxxCommerceStarterKit.Web.Models.PageTypes;

namespace OxxCommerceStarterKit.Web.Models.ViewModels
{
    public class HomePageViewModel : PageViewModel<HomePage>
    {

        public HomePageViewModel(HomePage currentPage) : base (currentPage)
        { }
        
    }
}
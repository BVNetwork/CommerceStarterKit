using System.Collections.Generic;
using System.Linq;
using OxxCommerceStarterKit.Web.Models.PageTypes;

namespace OxxCommerceStarterKit.Web.Models.ViewModels
{
    public class HomePageViewModel : PageViewModel<HomePage>
    {

        public HomePageViewModel(HomePage currentPage) : base (currentPage)
        { }

        public List<ProductListViewModel> RecommendationsForHomePage { get; set; }

        public bool ShowRecommendations
        {
            get
            {
                return !string.IsNullOrWhiteSpace(CurrentPage.RecommendationsHeader) &&
                       CurrentPage.RecommendationSection != null &&
                       RecommendationsForHomePage != null &&
                       RecommendationsForHomePage.Any();
            }
        }
    }
}
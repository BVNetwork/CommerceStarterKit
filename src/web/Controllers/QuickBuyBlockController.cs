using System;
using System.Web.Mvc;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Web;
using EPiServer.Web.Mvc;
using OxxCommerceStarterKit.Web.Business.ClientTracking;
using OxxCommerceStarterKit.Web.ModelBuilders;
using OxxCommerceStarterKit.Web.Models.Blocks;
using OxxCommerceStarterKit.Web.Models.ViewModels;

namespace OxxCommerceStarterKit.Web.Controllers
{
    public class QuickBuyBlockController : Controller, IRenderTemplate<QuickBuyBlock>
    {
        private readonly IGoogleAnalyticsTracker _googleAnalyticsTracker;
        private readonly IQuickBuyModelBuilder _modelBuilder;

        public QuickBuyBlockController(IGoogleAnalyticsTracker googleAnalyticsTracker, IQuickBuyModelBuilder modelBuilder)
        {
            _googleAnalyticsTracker = googleAnalyticsTracker;
            _modelBuilder = modelBuilder;
        }

        public ActionResult Index(QuickBuyBlock currentBlock)
        {
            QuickBuyViewModel model = new QuickBuyViewModel();
            model = _modelBuilder.Build(currentBlock, model);

            if (string.IsNullOrEmpty(currentBlock.PromotionId) == false ||
                string.IsNullOrEmpty(currentBlock.PromotionName) == false)
            {
                _googleAnalyticsTracker.TrackPromotionImpression(currentBlock.PromotionId, currentBlock.PromotionName, currentBlock.PromotionBannerName);
            }

            return PartialView("Index", model);
        }    

        [HttpPost]
        public ActionResult Index(QuickBuyBlock currentBlock, QuickBuyViewModel model)
        {
            return Content("Update done");
        }
    }
}
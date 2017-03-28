using System;
using System.Web.Mvc;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Web;
using EPiServer.Web.Mvc;
using Mediachase.Commerce.Customers;
using OxxCommerceStarterKit.Core.Models;
using OxxCommerceStarterKit.Core.Services;
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
        private readonly IOrderService _orderService;
        private readonly LocalizationService _localization;
        private readonly ICookieService _cookieService;

        public QuickBuyBlockController(IGoogleAnalyticsTracker googleAnalyticsTracker, IQuickBuyModelBuilder modelBuilder, IOrderService orderService, LocalizationService localization, ICookieService cookieService)
        {
            _googleAnalyticsTracker = googleAnalyticsTracker;
            _modelBuilder = modelBuilder;
            _orderService = orderService;
            _localization = localization;
            _cookieService = cookieService;
        }

        
        public ActionResult Index(QuickBuyBlock currentBlock)
        {
            var cookieModel = _cookieService.GetFromCookie();
            QuickBuyViewModel model = new QuickBuyViewModel(cookieModel);            

            model = _modelBuilder.Build(currentBlock, model);

            if (string.IsNullOrEmpty(currentBlock.PromotionId) == false ||
                string.IsNullOrEmpty(currentBlock.PromotionName) == false)
            {
                _googleAnalyticsTracker.TrackPromotionImpression(currentBlock.PromotionId, currentBlock.PromotionName, currentBlock.PromotionBannerName);
            }

            return PartialView(model);
        }
        
        [HttpPost]
        public ActionResult Index(QuickBuyBlock currentBlock, QuickBuyViewModel model)
        {
            _cookieService.SaveCookie(model);
            model = _modelBuilder.Build(currentBlock, model);
            model.Sku = model.SelectedSku;
            var order = _orderService.QuickBuyOrder(model, CustomerContext.Current.CurrentContactId);
            model.Success = true;
            model.OrderNumber = order.TrackingNumber;            
            return Content( GetSuccessMarkup(order.TrackingNumber));
        }

        private string GetSuccessMarkup(string trackingNumber)
        {
            return string.Format(@"<div class=""row padding-bottom"">
                        <div class=""col-sm-12"">
                            <div class=""alert alert-success alert-dismissible show"" role=""alert"">
                                <button type = ""button"" class=""close"" data-dismiss=""alert"" aria-label=""Close"">
                                    <span aria-hidden=""true"">&times;</span>
                                </button>
                                <strong>Success!</strong> {0} {1}
                            </div>
                        </div>
                    </div>" , _localization.GetString("/common/quickbuy/form/success"), trackingNumber);
        }
    }
}
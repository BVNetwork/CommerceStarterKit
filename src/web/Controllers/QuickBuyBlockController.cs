using System;
using System.Diagnostics;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Editor;
using EPiServer.Framework.Localization;
using EPiServer.Logging;
using EPiServer.Web;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
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
        private readonly IContentLoader _contentLoader;
        private readonly LocalizationService _localization;
        private readonly UrlResolver _urlResolver;
        private readonly ICookieService _cookieService;
        private static readonly ILogger _logger = LogManager.GetLogger();

        public QuickBuyBlockController(IGoogleAnalyticsTracker googleAnalyticsTracker, 
            IQuickBuyModelBuilder modelBuilder, 
            IOrderService orderService, 
            IContentLoader contentLoader,
            LocalizationService localization, 
            UrlResolver urlResolver,
            ICookieService cookieService)
        {
            _googleAnalyticsTracker = googleAnalyticsTracker;
            _modelBuilder = modelBuilder;
            _orderService = orderService;
            _contentLoader = contentLoader;
            _localization = localization;
            _urlResolver = urlResolver;
            _cookieService = cookieService;
        }

        
        public ActionResult Index(QuickBuyBlock currentBlock)
        {
            
            var tag = ControllerContext.ParentActionViewContext.ViewData["tag"] as string;
            
            _logger.Warning("Tag = " + (tag ?? "Tag is empty"));

            var cookieModel = _cookieService.GetFromCookie();
            QuickBuyViewModel model = new QuickBuyViewModel(cookieModel);            

            model = _modelBuilder.Build(currentBlock, model);

            if (string.IsNullOrEmpty(currentBlock.PromotionId) == false ||
                string.IsNullOrEmpty(currentBlock.PromotionName) == false)
            {
                _googleAnalyticsTracker.TrackPromotionImpression(currentBlock.PromotionId, currentBlock.PromotionName, currentBlock.PromotionBannerName);
            }

            if (tag == WebGlobal.ContentAreaTags.FullWidth)

            {
                return PartialView("FullWidth",model);
            }
            if (tag == WebGlobal.ContentAreaTags.HalfWidth || tag == WebGlobal.ContentAreaTags.OneThirdWidth)
            {
                return PartialView("Narrow", model);
            }

            return PartialView(model);
        }
        
        [HttpPost]
        public ActionResult PlaceOrder(QuickBuyBlock currentBlock, QuickBuyViewModel model)
        {
            try
            {
                model = _modelBuilder.Build(currentBlock, model);

                var page = _contentLoader.Get<PageData>(new ContentReference(model.SuccessUrl));
                
                _logger.Debug("Saving information");
                    _cookieService.SaveCookie(model);
                    model.Sku = model.SelectedSku;
                    var order = _orderService.QuickBuyOrder(model, CustomerContext.Current.CurrentContactId);
                    model.Success = true;
                    model.OrderNumber = order.TrackingNumber;

                var url = _urlResolver.GetUrl(page.ContentLink) + "?order=" + order.TrackingNumber;

                return Json(url);                
            }
            catch (Exception ex)
            {
                return Content(GetErrorMarkup(ex));
            }
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
        private string GetErrorMarkup(Exception ex)
        {
            return string.Format(@"<div class=""row padding-bottom"">
                        <div class=""col-sm-12"">
                            <div class=""alert alert-danger alert-dismissible show"" role=""alert"">
                                <button type = ""button"" class=""close"" data-dismiss=""alert"" aria-label=""Close"">
                                    <span aria-hidden=""true"">&times;</span>
                                </button>
                                <strong>Fail!</strong> {0} <br/>{1}
                            </div>
                        </div>
                    </div>", ex.Message, ex.StackTrace);
        }
    }
}
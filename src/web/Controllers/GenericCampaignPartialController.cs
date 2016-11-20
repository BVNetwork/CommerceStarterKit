using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPiServer.Commerce.Marketing;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;

namespace OxxCommerceStarterKit.Web.Controllers
{
    [TemplateDescriptor(Default = true)]
    public class GenericCampaignPartialController : PartialContentController<SalesCampaign>
    {
        private readonly IPromotionEngine _promotionEngine;

        public GenericCampaignPartialController(IPromotionEngine promotionEngine)
        {
            _promotionEngine = promotionEngine;
        }
        public override ActionResult Index(EPiServer.Commerce.Marketing.SalesCampaign currentContent)
        {
            return PartialView("CampaignPartials/GenericCampaign", currentContent);
        }
    }
}
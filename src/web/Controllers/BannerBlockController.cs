/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System.Web.Mvc;
using EPiServer.Web.Mvc;
using OxxCommerceStarterKit.Web.Business.ClientTracking;
using OxxCommerceStarterKit.Web.Models.Blocks;

namespace OxxCommerceStarterKit.Web.Controllers
{
    public class BannerBlockController : BlockController<BannerBlock>
    {
        private readonly IGoogleAnalyticsTracker _googleAnalyticsTracker;

        public BannerBlockController(IGoogleAnalyticsTracker googleAnalyticsTracker)
        {
            _googleAnalyticsTracker = googleAnalyticsTracker;
        }

        public override ActionResult Index(BannerBlock currentBlock)
        {
            if(string.IsNullOrEmpty(currentBlock.PromotionId) == false ||
               string.IsNullOrEmpty(currentBlock.PromotionName) == false)
            {
                _googleAnalyticsTracker.TrackPromotionImpression(currentBlock.PromotionId, currentBlock.PromotionName, currentBlock.PromotionBannerName);
            }

            return View("_bannerblock", currentBlock);
        }


    }
}

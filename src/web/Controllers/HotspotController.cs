using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPiServer.Find;
using EPiServer.Find.Api;
using EPiServer.Find.Framework;
using OxxCommerceStarterKit.Web.Models.FindModels;
using OxxCommerceStarterKit.Web.Models.ViewModels;

namespace OxxCommerceStarterKit.Web.Controllers
{
    public class HotspotController : Controller
    {
        // GET: Hotspot
        public ActionResult Index(int id)
        {
            HotspotViewModel model = new HotspotViewModel();
            var product = GetProductByEntityId(id);
            if (product != null)
            {
                model.Title = product.DisplayName;
                model.ImageUrl = product.DefaultImageUrl;
                model.Url = product.ProductUrl;
            }
            else
            {
                model.Title = "Not Found";
                model.ImageUrl = "";
                model.Url = "";
            }
            return View(model);
        }

        protected FindProduct GetProductByEntityId(int id)
        {
            SearchResults<FindProduct> results = SearchClient.Instance.Search<FindProduct>()
                    .Filter(p => p.InRiverEntityId.Match(id))
                    .GetResult();
            if (results.Hits.Any())
            {
                // Pick the first one
                SearchHit<FindProduct> product = results.Hits.FirstOrDefault();
                return product.Document;
            }
            return null;

        }


    }
}
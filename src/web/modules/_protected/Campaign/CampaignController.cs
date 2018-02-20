using System.Configuration;
using System.Web.Mvc;

namespace OxxCommerceStarterKit.Web.modules._protected.Campaign
{
    public class CampaignController : Controller
    {
        public ActionResult Index(string id)
        {
            var action = id;
            if (string.IsNullOrEmpty(action))
            {
                action = "overview";
            }
            var model = new CampaignViewModel
            {
                FrameSource = ConfigurationManager.AppSettings["CampaignBaseUrl"],
                Action = action
            };

            return View("~/modules/_protected/Campaign/Views/Index.cshtml", model);
        }
    }
}
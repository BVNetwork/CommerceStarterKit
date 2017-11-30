using System.Configuration;
using System.Web.Mvc;

namespace OxxCommerceStarterKit.Web.modules._protected.Perform
{
    public class PerformController : Controller
    {
        public ActionResult Index(string id)
        {
            var action = id;
            if (string.IsNullOrEmpty(action))
            {
                action = "overview";
            }
            var model = new PerformViewModel
            {
                FrameSource = ConfigurationManager.AppSettings["episerver:personalization.BaseApiUrl"],
                Action = action
            };

            return View("~/modules/_protected/Perform/Views/Index.cshtml", model);
        }
    }
}
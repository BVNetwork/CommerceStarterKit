using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;
using JOS.ContentSerializer;
using Newtonsoft.Json;

namespace OxxCommerceStarterKit.Web.Business.JsonChannel
{
    [TemplateDescriptor(TagString= "Headless", AvailableWithoutTag= false, Inherited= true)]
    public class JsonPageDataController : PageController<PageData>
    {

        public ActionResult Index(PageData currentPage)
        {
            var json = ServiceLocator.Current.GetInstance<IContentJsonSerializer>().GetStructuredData(currentPage);
            var result = JsonConvert.SerializeObject(json, Formatting.Indented);
            return Content(result, "application/json");
        }
    }
}
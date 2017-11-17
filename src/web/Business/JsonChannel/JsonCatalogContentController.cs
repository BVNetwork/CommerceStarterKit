using System.Web.Mvc;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Cms;
using EPiServer.Find.Framework;
using EPiServer.Framework.DataAnnotations;
using Newtonsoft.Json;
using OxxCommerceStarterKit.Web.Controllers;
using OxxCommerceStarterKit.Web.Models.FindModels;

namespace OxxCommerceStarterKit.Web.Business.JsonChannel
{
    [TemplateDescriptor(TagString = "Headless", AvailableWithoutTag = false, Inherited = true)]
    public class JsonCatalogContentController : CommerceControllerBase<CatalogContentBase>
    {
      
        public ActionResult Index(CatalogContentBase currentContent, PageData currentPage)
        {
            var product = SearchClient.Instance.Search<FindProduct>()
                .Filter(x => x.Id.Match(currentContent.ContentLink.ID))
                .Filter(x => x.Language.Match(currentPage.Language.TwoLetterISOLanguageName))
                .Select(x => new
                {
                    Properties = x,
                    Description = x.Description.AsCropped(500),
                    Overview = x.Overview.AsCropped(500)
                }).GetResult();

            var result = JsonConvert.SerializeObject(product, Formatting.Indented);
            return Content(result, "application/json");
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Xml;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Configuration;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Cms;
using EPiServer.Find.Framework;
using EPiServer.Web;
using OxxCommerceStarterKit.Web.Models.FindModels;
using OxxCommerceStarterKit.Web.Models.PageTypes;

namespace OxxCommerceStarterKit.Web.Api
{
    public class ProductInfoController : ApiController
    {
        private readonly IContentLoader _contentLoader;

        public ProductInfoController(IContentLoader contentLoader)
        {
            _contentLoader = contentLoader;
        }

        [HttpGet]
        public HttpResponseMessage Get(string id)
        {
            if(string.IsNullOrWhiteSpace(id))
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            if (id.Contains(':'))
            {
                id = GetCampaignProductId(id);
            }

            var result = SearchClient.Instance.Search<FindProduct>()
                .Filter(x => x.Language.Match("en"))
                .Filter(x => x.Code.MatchCaseInsensitive(id))
                .Select(x => new ProjectedProduct
                    {
                        Code = x.Code,
                        Name = x.Name,
                        Overview = x.Overview.AsCropped(450),
                        Description = x.Description.AsCropped(450),
                        DefaultImageUrl = x.DefaultImageUrl,
                        ProductUrl = x.ProductUrl,
                        DiscountedPrice = x.DiscountedPrice,
                        DefaultPrice = x.DefaultPrice,
                        ParentCategoryName = x.ParentCategoryName
                    }
                )
                .GetResult();

            if (result.Any())
            {

                var product = result.FirstOrDefault();

                if (product != null)
                {
                    var xml = CreateXml(product);

                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(xml, Encoding.UTF8, "application/xml")
                    };
                }
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);           
        }

        private class ProjectedProduct
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public string Overview { get; set; }
            public string Description { get; set; }
            public string DefaultImageUrl { get; set; }
            public string ProductUrl { get; set; }
            public string DiscountedPrice { get; set; }
            public string DefaultPrice { get; set; }
            public List<string> ParentCategoryName { get; set; }
        }

        private static string CreateXml(ProjectedProduct product)
        {
            var siteUrl = SiteDefinition.Current.SiteUrl.ToString();

            if(siteUrl.EndsWith("/"))
            {
                siteUrl = siteUrl.Substring(0, siteUrl.Length - 2);
            }

            XmlDocument doc = new XmlDocument();

            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);

            XmlNode elementNode = doc.CreateElement("element");
            doc.AppendChild(elementNode);

            AddProperty(doc, "id", product.Code, elementNode);
            AddProperty(doc, "name", product.Name, elementNode);
            AddProperty(doc, "category", String.Join("#", product.ParentCategoryName), elementNode);
            AddProperty(doc, "text1", product.Name, elementNode);
            AddProperty(doc, "text2", product.Description, elementNode);

            if (string.IsNullOrEmpty(product.DiscountedPrice) == false)
            {
                AddProperty(doc, "text5", product.DiscountedPrice, elementNode);
                AddProperty(doc, "text6", product.DefaultPrice, elementNode);
            }
            else
            {
                AddProperty(doc, "text5", product.DefaultPrice, elementNode);
            }

            AddProperty(doc, "link1Url", siteUrl + product.ProductUrl, elementNode);
            AddProperty(doc, "link1Text", "Read more", elementNode);
            AddProperty(doc, "image1ImageUrl", siteUrl + product.DefaultImageUrl, elementNode);
            AddProperty(doc, "image1Link", siteUrl + product.ProductUrl, elementNode);
            AddProperty(doc, "image1AltText", product.Name, elementNode);

            return XmlToString(doc);
        }

        private string GetCampaignProductId(string id)
        {
            var index = int.Parse(id.Substring(id.LastIndexOf(":") + 1));

            var startPage = _contentLoader.Get<HomePage>(ContentReference.StartPage);

            var children = _contentLoader.GetChildren<EntryContentBase>(startPage.CampaginCategory);

            if (children != null && children.Any())
            {
                return children.Skip(index - 1).FirstOrDefault().Code;
            }

            return null;
        }


        private static void AddProperty(XmlDocument doc, string name, string value, XmlNode elementNode)
        {
            XmlNode productNode = doc.CreateElement("property");
            XmlAttribute productAttribute = doc.CreateAttribute("name");
            productAttribute.Value = name;
            productNode.Attributes.Append(productAttribute);
            productNode.AppendChild(doc.CreateTextNode(value));
            elementNode.AppendChild(productNode);
        }

        private static string XmlToString(XmlDocument doc)
        {
            var xml = string.Empty;
            using (var stringWriter = new StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringWriter))
            {
                doc.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                xml = stringWriter.GetStringBuilder().ToString();
            }

            return xml;
        }
    }
}
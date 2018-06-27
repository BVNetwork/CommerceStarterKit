using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Cms;
using EPiServer.Find.Framework;
using EPiServer.Security;
using EPiServer.Shell.Services.Rest;
using OxxCommerceStarterKit.Web.Extensions;
using OxxCommerceStarterKit.Web.Models.FindModels;

namespace OxxCommerceStarterKit.Web.Api
{
    public class ProductInfoController : ApiController
    {
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get(string id)
        {

            var result = SearchClient.Instance.Search<FindProduct>()
                .Filter(x => x.Language.Match("en"))
                .Filter(x => x.Code.MatchCaseInsensitive(id))
                .Select(x => new
                    {
                        x.Code,
                        x.Name,
                        Overview = x.Overview.AsCropped(450),
                        Description = x.Description.AsCropped(450),
                        x.DefaultImageUrl,
                        x.ProductUrl,
                        x.DiscountedPrice,
                        x.DefaultPrice,
                        x.ParentCategoryName
                    }
                )
                .GetResult();

            if (result.Any())
            {


                var product = result.FirstOrDefault();



                XmlDocument doc = new XmlDocument();

                XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.AppendChild(docNode);

                XmlNode elementNode = doc.CreateElement("element");
                doc.AppendChild(elementNode);

                AddProperty(doc, "id", product.Code, elementNode); 
                AddProperty(doc, "name", product.Name, elementNode);
                AddProperty(doc, "category", string.Join("#", product.ParentCategoryName), elementNode);
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
                               
                AddProperty(doc, "link1Url", product.ProductUrl, elementNode);
                AddProperty(doc, "link1Text", "Read more", elementNode);                
                AddProperty(doc, "image1ImageUrl", product.DefaultImageUrl, elementNode);

                var xml = CreateXml(doc);

                return new HttpResponseMessage()
                {
                    Content = new StringContent(xml, Encoding.UTF8, "application/xml")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);           
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

        private static string CreateXml(XmlDocument doc)
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
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;

namespace OxxCommerceStarterKit.Web.Business.Rss
{
    public static class SyndicationFeedExtensions
    {
       
        /// <summary>
        /// Set enclosure for rss 2.0
        /// </summary>
        /// <param name="item">The feed entry.</param>
        /// <param name="url">The thumbnail URL.</param>
        public static void SetEnclosure(this SyndicationItem item, string url)
        {
            item.ElementExtensions.Add(new SyndicationElementExtension(
                new XElement("enclosure", new XAttribute("url", url), new XAttribute("type", "image/jpeg"))));
        }
    }
}
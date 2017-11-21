using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;

namespace OxxCommerceStarterKit.Web.Business.Rss
{
    public static class SyndicationFeedExtensions
    {
        private const string YahooMediaNamespacePrefix = "media";
        private const string YahooMediaNamespace = "http://search.yahoo.com/mrss/";

        /// <summary>
        /// Adds a namespace to the specified feed.
        /// </summary>
        /// <param name="feed">The syndication feed.</param>
        /// <param name="namespacePrefix">The namespace prefix.</param>
        /// <param name="xmlNamespace">The XML namespace.</param>
        public static void AddNamespace(this SyndicationFeed feed, string namespacePrefix, string xmlNamespace)
        {
            feed.AttributeExtensions.Add(
                new XmlQualifiedName(namespacePrefix, XNamespace.Xmlns.ToString()),
                xmlNamespace);
        }

        /// <summary>
        /// Adds the yahoo media namespace to the specified feed.
        /// </summary>
        /// <param name="feed">The syndication feed.</param>
        public static void AddYahooMediaNamespace(this SyndicationFeed feed)
        {
            AddNamespace(feed, YahooMediaNamespacePrefix, YahooMediaNamespace);
        }
        
        /// <summary>
        /// Sets the Yahoo Media thumbnail for the feed entry.
        /// </summary>
        /// <param name="item">The feed entry.</param>
        /// <param name="url">The thumbnail URL.</param>
        /// <param name="width">The optional width of the thumbnail image.</param>
        /// <param name="height">The optional height of the thumbnail image.</param>
        public static void SetMediaContent(this SyndicationItem item, string url)
        {
            XNamespace ns = YahooMediaNamespace;
            item.ElementExtensions.Add(new SyndicationElementExtension(
                new XElement(
                    ns + "content",
                    new XAttribute("url", url))));
        }
    }
}
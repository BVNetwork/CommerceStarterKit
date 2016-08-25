using System.Collections.Generic;
using EPiServer.Core;
using OxxCommerceStarterKit.Interfaces;

namespace OxxCommerceStarterKit.Sannsyn
{
    public class ProductRecommendations : IRecommendations
    {
        public ProductRecommendations(string recommenderName, IEnumerable<IContent> products)
        {
            RecommenderName = recommenderName;
            Products = products;
        }

        public IEnumerable<IContent> Products { get; set; }

        public string RecommenderName { get; set; }
    }
}
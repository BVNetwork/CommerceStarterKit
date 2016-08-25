using System.Collections.Generic;
using EPiServer.Core;
using OxxCommerceStarterKit.Interfaces;

namespace OxxCommerceStarterKit.Web.Services
{
    public class Recommendations : IRecommendations
    {
        public Recommendations(string recommenderName, IEnumerable<IContent> products)
        {
            RecommenderName = recommenderName;
            Products = products;
        }


        public IEnumerable<IContent> Products { get; }
        public string RecommenderName { get; }
    }
}
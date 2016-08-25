using System.Collections.Generic;
using EPiServer.Core;
using OxxCommerceStarterKit.Interfaces;

namespace OxxCommerceStarterKit.Sannsyn
{
    public class ProductRecommendations : IRecommendations
    {
        private readonly IEnumerable<IContent> _products;
        private readonly string _recommenderName;

        public ProductRecommendations(string recommenderName, IEnumerable<IContent> products)
        {
            _recommenderName = recommenderName;
            _products = products;
        }

        public IEnumerable<IContent> Products
        {
            get { return _products; }
        }

        public string RecommenderName
        {
            get { return _recommenderName; }
        }
    }
}
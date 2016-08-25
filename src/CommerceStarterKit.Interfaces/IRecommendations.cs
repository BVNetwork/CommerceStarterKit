using System.Collections.Generic;
using EPiServer.Core;

namespace OxxCommerceStarterKit.Interfaces
{
    public interface IRecommendations
    {
        IEnumerable<IContent> Products { get; }
        string RecommenderName { get; }
    }
}
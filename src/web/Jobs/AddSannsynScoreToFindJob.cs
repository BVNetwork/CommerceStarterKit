using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.Find;
using EPiServer.Find.Framework;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using EPiServer.ServiceLocation;
using OxxCommerceStarterKit.Interfaces;
using OxxCommerceStarterKit.Web.Models.FindModels;

namespace OxxCommerceStarterKit.Web.Jobs
{
    [ScheduledPlugIn(DisplayName = "Add Sannsyn score to Find")]
    public class AddSannsynScoreToFindJob : ScheduledJobBase
    {
        public override string Execute()
        {
            int scoreAddedToProducts = 0;
            IClient client = SearchClient.Instance;
            var recommendedProductService = ServiceLocator.Current.GetInstance<IRecommendedProductsService>();
            // TODO: Remove hardcoded number of max products we can get a score for
            Dictionary<string, double> scoreForItems = recommendedProductService.GetScoreForItems(10000);
            if (scoreForItems != null)
            {
                List<string> codes = scoreForItems.Keys.ToList();
                int numOfItems = 100;
                for (int i = 0; i < codes.Count(); i = i + numOfItems)
                {
                    var items = codes.Skip(i).Take(numOfItems);
                    var searchResults = client.Search<FindProduct>()
                        .Filter(x => x.Code.In(items))
                        .Take(1000)//Can be on more than one langage, so take must be larger than number in list
                        .GetResult();
                    var findProducts = searchResults.ToList();
                    List<FindProduct> productsToIndex = new List<FindProduct>();
                    foreach (var findProduct in findProducts)
                    {
                        findProduct.Score = scoreForItems[findProduct.Code];
                        productsToIndex.Add(findProduct);
                        scoreAddedToProducts++;
                    }
                    client.Index(productsToIndex);
                }
              
            }
            return string.Format("Score from Sannsyn added to {0} products", scoreAddedToProducts);


        }
    }
}
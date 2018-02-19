/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using EPiServer;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Framework;
using EPiServer.Framework.Localization;
using EPiServer.Logging;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Pricing;
using OxxCommerceStarterKit.Web.Models.Blocks.Contracts;
using OxxCommerceStarterKit.Web.Models.FindModels;
using CsvHelper;
using System.IO;
using EPiServer.Find.Cms;
using EPiServer.Find.Helpers.Text;

namespace OxxCommerceStarterKit.Web.Jobs
{
    [ScheduledPlugIn(DisplayName = "Export Product Catalog to Campaign")]
	public class CreateCampaignProductsCsvFile : ScheduledJobBase
    {
		class IndexInformation
		{
			public IndexInformation()
			{
				MachineName = Environment.MachineName;

			}
			public int NumberOfProductsIndexed { get; set; }
			public long Duration { get; set; }
			public string MachineName { get; set; }
			public int NumberOfProductsRemoved { get; set; }
			public int NumberOfProductsFound { get; set; }
			public int NumberOfProductsInIndex { get; set; }
			public int NumberOfProductsFoundAfterExpiredFilter { get; set; }

			public override string ToString()
			{
				return string.Format("Found {6}/{4}, indexed {0} and removed {3}/{5} products in {1}ms on {2}", NumberOfProductsIndexed, Duration,
					MachineName, NumberOfProductsRemoved, NumberOfProductsFound, NumberOfProductsInIndex, NumberOfProductsFoundAfterExpiredFilter);
			}
		}


		private bool _stopSignaled;
		readonly ReferenceConverter referenceConverter = ServiceLocator.Current.GetInstance<ReferenceConverter>();
		readonly IContentLoader contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
        protected static ILogger _log = LogManager.GetLogger();

        public CreateCampaignProductsCsvFile()
		{
			IsStoppable = true;
		}

		/// <summary>
		/// Called when a user clicks on Stop for a manually started job, or when ASP.NET shuts down.
		/// </summary>
		public override void Stop()
		{
			_stopSignaled = true;
		}

		/// <summary>
		/// Starts the job
		/// </summary>
		/// <returns>A status message that will be logged</returns>
		public override string Execute()
		{
			IndexInformation info = new IndexInformation();
			Stopwatch tmr = Stopwatch.StartNew();

            
			var language = LanguageSelector.MasterLanguage();
			var localizationService = ServiceLocator.Current.GetInstance<LocalizationService>();
			var marketService = ServiceLocator.Current.GetInstance<IMarketService>();
			var allMarkets = marketService.GetAllMarkets();
			var priceService = ServiceLocator.Current.GetInstance<IPriceService>();
			var linksRepository = ServiceLocator.Current.GetInstance<ILinksRepository>();


            // TODO: Add support for multiple catalogs. This will pick the first one.
            IEnumerable<ContentReference> contentLinks = contentLoader.GetDescendents(Root);

            List<ProductInfo> productsForExport = new List<ProductInfo>();
			int bulkSize = 100;
			foreach (CultureInfo availableLocalization in localizationService.AvailableLocalizations)
			{
				var market = allMarkets.FirstOrDefault(m => m.DefaultLanguage.Equals(availableLocalization));
                // IMPORTANT! We only support English for now
                if (market == null || string.Compare(market.DefaultLanguage.TwoLetterISOLanguageName, "en", StringComparison.InvariantCultureIgnoreCase) != 0)
				{
					continue;
				}
				string language2 = availableLocalization.Name.ToLower();
				

				int allContentsCount = contentLinks.Count();
				for (var i = 0; i < allContentsCount; i += bulkSize)
				{
					var items = contentLoader.GetItems(contentLinks.Skip(i).Take(bulkSize), new LanguageSelector(availableLocalization.Name));
					var items2 = items.OfType<IIndexableContent>().ToList();

					foreach (var content in items2)
					{
						info.NumberOfProductsFound++;

						OnStatusChanged(String.Format("Exporting product {0} of {1} - {2}", i + 1, allContentsCount, content.Name));
                        
                        if (content.ShouldIndex())
                        {
							info.NumberOfProductsFoundAfterExpiredFilter++;

                            FindProduct findProduct = null;
                            try
                            {
                                findProduct = content.GetFindProduct(market);
                            }
                            catch (Exception ex)
                            {
                                string msg = string.Format("Cannot generate FindProduct for {0}", content.Name);
                                _log.Error(msg, ex);
                            }

							if (findProduct != null)
							{
                                productsForExport.Add(new ProductInfo(findProduct));
								info.NumberOfProductsIndexed++;							
							}
						}

						//For long running jobs periodically check if stop is signaled and if so stop execution
						if (_stopSignaled)
						{
							tmr.Stop();
							info.Duration = tmr.ElapsedMilliseconds;
							break;
						}

					}

					//For long running jobs periodically check if stop is signaled and if so stop execution
					if (_stopSignaled)
					{
						tmr.Stop();
						info.Duration = tmr.ElapsedMilliseconds;
						break;
					}

				}
			
			}

			if (_stopSignaled)
			{
				return "Stop of job was called. " + info.ToString();
			}


			tmr.Stop();
			info.Duration = tmr.ElapsedMilliseconds;

            // Export

            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/products.csv"); // "c:\\temp\\products.csv"; // System.Web.HttpContext.Current.Server.MapPath("~/App_Data/products.csv");
            using (StreamWriter writer = new StreamWriter(path, false))
            {
                var csv = new CsvWriter(writer);
                csv.Configuration.Delimiter = ";";
                csv.WriteHeader(typeof(ProductInfo));
                csv.NextRecord();
                csv.WriteRecords(productsForExport);
                csv.Flush();
                csv.Dispose();
            }

            return info.ToString();
		}


		public ContentReference Root
		{
		    get
		    {
		        var ids = GetCatalogIds().ToList();
                if(ids.Any())
                {
                    return referenceConverter.GetContentLink(ids.First(), CatalogContentType.Catalog, 0);
                }

                return ContentReference.EmptyReference;

            }
		}

        protected IEnumerable<int> GetCatalogIds()
        {
            ICatalogSystem catalogSystem = ServiceLocator.Current.GetInstance<ICatalogSystem>();
            CatalogDto catalogDto = catalogSystem.GetCatalogDto();
            foreach (CatalogDto.CatalogRow row in catalogDto.Catalog)
            {
                yield return row.CatalogId;
            }

        }

    }

    class ProductInfo
    {
        public ProductInfo()
        {
            //text1 = "text 1";
            //text2 = "text 2";
            //text3 = "text 3";
            //text4 = "text 4";
            //text5 = "text 5";
            //text6 = "text 6";
            //text7 = "text 7";
            //text8 = "text 8";
            //text9 = "text 9";
            //text10 = "text 10";
        }

        public ProductInfo(FindProduct findProduct) : this()
        {
            id = findProduct.Code;
            name = findProduct.Name;
            findProduct.ParentCategoryName.Reverse();
            category = string.Join("#", findProduct.ParentCategoryName);
            text1 = findProduct.Name;
            if (findProduct.Description != null && findProduct.Description.IsEmpty == false)
            {
                text2 = findProduct.Description.AsViewedByAnonymous();
                text2 = text2.StripHtml();
                if (text2.Length > 450)
                {
                    text2 = text2.Substring(0, 450);
                }

            }
            if (string.IsNullOrEmpty(findProduct.DiscountedPrice) == false)
            {
                text5 = findProduct.DiscountedPrice;
                text6 = findProduct.DefaultPrice;
            }
            else
            {
                text5 = findProduct.DefaultPrice;
            }
            link1Text = "Read More";
            link1Url = findProduct.ProductUrl;
            image1ImageUrl = findProduct.DefaultImageUrl;
            image1Link = findProduct.ProductUrl;
            image1AltText = name;

        }

        public ProductInfo(string Id, string name, string category, string displayName, string intro, string price, string oldPrice) : this()
        {
            this.id = id;
            this.name = name;
            this.category = category;
            text1 = displayName;
            text2 = intro;
            text5 = price;
            text6 = oldPrice;
        }
        /*
            Example
            id;name;category;text1;text2;text3;text4;text5;text6;text7;text8;text9;text10;link1Text;link1Url;link2Text;link2Url;link3Text;link3Url;image1ImageUrl;image1AltText;image1Link;image2ImageUrl;image2AltText;image2Link;image3ImageUrl;image3AltText;image3Link;image4ImageUrl;image4AltText;image4Link;image5ImageUrl;image5AltText;image5Link;image6ImageUrl;image6AltText;image6Link
            canon-5d-m3;Canon EOS 5D Mark III;Photo#Cameras#DSLR;Canon EOS 5D Mark III;With supercharged EOS performance and stunning full frame, high-resolution image capture, the EOS 5D Mark III is designed to perform.;;;4500;5000;Canon;;;;More Information;https://www.epicphoto.no/en/starterkit/photo/cameras/dslr/canon-5d-m3/;;;;;https://www.epicphoto.no/globalassets/catalogs/photo/cameras/slr/canon/5dmarkiii/eos-5d-m3-6.jpg?preset=width320;Canon EOS 5D Mark III;;;;;;;;;;;;;;;;
        
            id;name;
            category;
            text1: Display Name
            text2: Intro text 
            text3;
            text4;
            text5: Price
            text6: Old Price
            text7;text8;text9;text10;
            link1Text: "More information"
            link1Url: product url
            link2Text;link2Url;link3Text;link3Url;
            image1ImageUrl;
            image1AltText;
            image1Link;
            image2ImageUrl;image2AltText;image2Link;image3ImageUrl;image3AltText;image3Link;image4ImageUrl;image4AltText;image4Link;image5ImageUrl;image5AltText;image5Link;image6ImageUrl;image6AltText;image6Link
             
        */
        public string id { get; set; }
        public string name { get; set; }
        public string category { get; set; }
        public string text1 { get; set; }
        public string text2 { get; set; }
        public string text3 { get; set; }
        public string text4 { get; set; }
        public string text5 { get; set; }
        public string text6 { get; set; }
        public string text7 { get; set; }
        public string text8 { get; set; }
        public string text9 { get; set; }
        public string text10 { get; set; }
        public string link1Text { get; set; }
        public string link1Url { get; set; }
        public string image1ImageUrl { get; set; }
        public string image1AltText { get; set; }
        public string image1Link { get; set; }
    }
}

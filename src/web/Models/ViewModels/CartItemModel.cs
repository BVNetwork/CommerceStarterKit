/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Web.Extensions;
using OxxCommerceStarterKit.Web.Models.Catalog;

namespace OxxCommerceStarterKit.Web.Models.ViewModels
{
	public class CartItemModel
	{
		// ReSharper disable UnassignedField.Compiler
		private Injected<UrlResolver> _urlResolver;
		private Injected<IPermanentLinkMapper> _permanentLinkMapper;
		// ReSharper restore UnassignedField.Compiler

		public CartItemModel(VariationContent entry)
		{
			Code = entry.Code;
		    Name = GetCleanString(entry.DisplayName);
			Entry = entry;
            var parent = entry.GetParent();
			var media = entry.GetCommerceMedia();
		    if (entry is FashionItemContent)
		    {
		        FashionItemContent fashionItemContent = (FashionItemContent) entry;
		        Size = fashionItemContent.Facet_Size;
		    }
            //TODO: Make this generic, move proerty to base?
		    else if(entry.Property["Size"] != null)
		    {
		        Size = (string)entry.Property["Size"].Value;
		    }
           

		    if (media != null)
		    {
		        ImageUrl = _urlResolver.Service.GetUrl(media.AssetContentLink(_permanentLinkMapper.Service));
		    }
            else if (parent != null)
		    {
                //If variant does not have images, we get image from product
                media = parent.GetCommerceMedia();
                if(media != null)
                    ImageUrl = _urlResolver.Service.GetUrl(media.AssetContentLink(_permanentLinkMapper.Service));
		    }
			
			if (parent != null)
			{
				ColorImageUrl = parent.AssetSwatchUrl();
				if (parent is FashionProductContent)
				{
					var fashionProductContent = (FashionProductContent)parent;
                    Color = fashionProductContent.FacetColor;
					ArticleNumber = fashionProductContent.Code;
					if (fashionProductContent.SizeAndFit != null && 
						fashionProductContent.SizeAndFit.ToHtmlString() != null)
					{
						Description = fashionProductContent.SizeAndFit.ToHtmlString().StripHtml().StripPreviewText(255);
					}
					else
					{
						Description = "";
					}
				}
			}
		}

        //TODO: Add more characters that should be replaced
        private string GetCleanString(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;
            name = name.Replace("'", "");
            return name;
        }

	    public string Name { get; set; }

	    public string Code
		{
			get;
			set;
		}

		public EntryContentBase Entry
		{
			get;
			set;
		}

		private bool? _canBuyEntry;
	    private string _buyButtonColor;

	    public bool CanBuyEntry
		{
			get {
				return Entry is VariationContent && _canBuyEntry.HasValue && _canBuyEntry.Value;
			}
			set
			{
				_canBuyEntry = value;
			}
		}


		public string ImageUrl { get; set; }
		public string ColorImageUrl { get; set; }
		public string Color { get; set; }
		public string Description { get; set; }
		public string ArticleNumber { get; set; }
		public string Size { get; set; }

	    public string BuyButtonColor
	    {
	        get
	        {
	            if (string.IsNullOrWhiteSpace(_buyButtonColor))
	                return "btn-primary";
	            return _buyButtonColor;
	        }
	        set { _buyButtonColor = value; }
	    }
	}
}

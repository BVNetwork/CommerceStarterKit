/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using CsQuery.Engine.PseudoClassSelectors;
using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Newtonsoft.Json;
using OxxCommerceStarterKit.Web.Models.Catalog;
using OxxCommerceStarterKit.Web.Models.Files;

namespace OxxCommerceStarterKit.Web.Models.ViewModels
{
	/// <summary>
	/// View model for the image file
	/// </summary>
	public class ImageViewModel
	{
		public ImageViewModel(ImageFile currentContent, string language)
		{
			// Note! Since we change the size of the images frequently by
			// adding /sizeName to the url, we get into trouble when EPiServer
			// renders the url in edit mode. Clicking an image in edit mode
			// renders the link to the actual ImageFile content, and we can't
			// just append the /sizeName to it. Because of this, we always
			// render the image for default view (not edit or preview)
			Url = ServiceLocator.Current.GetInstance<UrlResolver>().GetUrl(
				currentContent.ContentLink,
				language,
				new VirtualPathArguments(){ContextMode = ContextMode.Default});

		    UniqueId = currentContent.ContentGuid.ToString().Replace("-", "");

			Name = currentContent.Name;
			Description = currentContent.Description;
			Copyright = currentContent.Copyright;
			Language = language;
		    MerchandiseImageMap = currentContent.ResourceImageMap;

			if (currentContent.Link != null)
			{
				ExternalLink = currentContent.Link.ToString();
				ExternalLinkTarget = currentContent.Link.IsAbsoluteUri ? "_blank" : "";
			}
			try
			{
				HotSpots = JsonConvert.DeserializeObject<IEnumerable<HotSpotContainer>>(currentContent.HotSpotSettings);
			}
			catch (Exception)
			{
				// TODO: Remove empty catch - needs testing	
			}
		}


        public string UniqueId { get; set; }
		public string Tag { get; set; }
		public bool IsMobile { get; set; }

		/// <summary>
		/// Gets or sets the URL to the image.
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// Link to another page
		/// </summary>
		public string ExternalLink { get; set; }

		/// <summary>
		/// if the link should open in its own window
		/// </summary>
		public string ExternalLinkTarget { get; set; }

		/// <summary>
		/// Gets or sets the name of the image.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Use for image alt text
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the copyright information of the image.
		/// </summary>
		public string Copyright { get; set; }

		public string Language { get; set; }

	    public string MerchandiseImageMap { get; set; }

	    public IEnumerable<HotSpotContainer> HotSpots { get; set; }

		public class HotSpotContainer
		{
			private IContentLoader _contentLoader;
			private IContent _content;

			public HotSpotContainer()
			{
				_contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
			}

			public string link { get; set; }
			public bool IsProduct
			{
				get
				{
					LoadContent();
					return _content != null && _content is FashionProductContent;
				}
			}
			private void LoadContent()
			{
				if (!string.IsNullOrEmpty(link) && _content == null)
				{
					_contentLoader.TryGet<IContent>(ContentReference.Parse(link), out _content);
				}
			}
			public ContentReference ContentLink
			{
				get
				{
					LoadContent();
					return _content != null ? _content.ContentLink : null;
				}
			}

			public HotSpot hotSpot { get; set; }
			public HotSpotArea area { get; set; }

			public class HotSpot
			{
				public decimal top { get; set; }
				public decimal left { get; set; }
				public int width { get; set; }
				public int height { get; set; }
			}

			public class HotSpotArea
			{
				public decimal top { get; set; }
				public decimal left { get; set; }
				public decimal width { get; set; }
				public decimal height { get; set; }
			}
		}

	}
}

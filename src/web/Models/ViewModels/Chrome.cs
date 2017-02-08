/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using EPiServer;
using EPiServer.Core;
using EPiServer.SpecializedProperties;
using OxxCommerceStarterKit.Web.Models.PageTypes;

namespace OxxCommerceStarterKit.Web.Models.ViewModels
{
    public class Chrome
    {
        public Chrome()
        {
            Languages = new List<ChromeLanguageInfo>();
        }

        public LinkItemCollection TopLeftMenu { get; set; }
        public LinkItemCollection TopRightMenu { get; set; }
        public IEnumerable<PageData> FooterMenu { get; set; }
        public ContentArea SocialMediaIcons { get; set; }
        public ContentArea FooterButtons { get; set; }
        public ContentReference LoginPage { get; set; }
        public PageReference AccountPage { get; set; }
        public string Language { get; set; }
        public PageReference CheckoutPage { get; set; }
        public Url LogoImageUrl { get; set; }
        public ContentReference SearchPage { get; set; }
        public IEnumerable<ChromeLanguageInfo> Languages { get; set; }
        public ContentArea GlobalFooterContent { get; set; }
        public string HomePageUrl { get; set; }
        public string ColorScheme { get; set; }
        public bool HideHeader { get; set; }
        public bool HideFooter { get; set; }

        //TODO: Remove after configuration
        public bool ShowWarning
        {
            get
            {
                bool showWarning = false;
                var value = ConfigurationManager.AppSettings["ShowWarning"];

                if (value != null)
                {
                    showWarning = Convert.ToBoolean(value);
                }

                return showWarning;
            }
        }

    }
}

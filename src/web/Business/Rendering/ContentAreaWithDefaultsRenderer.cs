/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Web.Mvc.Html;

namespace OxxCommerceStarterKit.Web.Business.Rendering
{
    /// <summary>
    /// Extends the default <see cref="ContentAreaRenderer"/> to apply custom CSS classes to each <see cref="ContentFragment"/>.
    /// </summary>
    public class ContentAreaWithDefaultsRenderer : ContentAreaRenderer
    {
        protected override string GetContentAreaItemCssClass(HtmlHelper htmlHelper, ContentAreaItem contentAreaItem)
        {
            var childrenCssClass = htmlHelper.ViewData["ChildrenCssClass"];
            if(childrenCssClass != null)
            {
                return childrenCssClass.ToString();
            }

            var tag = GetContentAreaItemTemplateTag(htmlHelper, contentAreaItem);

            var content = contentAreaItem.GetContent();
            if(content != null)
            {
                if (tag == null )
                {
                    // Let block decide what to use as default if not
                    // specified on the content area itself
                    tag = GetDefaultDisplayOption(content);
                    //if (tag == null)
                    //{
                    //    // Default is always the smallest one we've got
                    //    tag = WebGlobal.ContentAreaTags.OneThirdWidth;
                    //}
                }
				htmlHelper.ViewContext.ViewData["Tag"] = tag;
                return string.Format("block {0} {1} {2}",
                    GetTypeSpecificCssClasses(content), 
                    GetCssClassForTag(tag), 
                    tag);
            }
            else
            {
                return WebGlobal.ContentAreaTags.NoRenderer;
            }
        }

        
        /// <summary>
        /// Checks each content item to see if it supports a default tag to use as
        /// size if the editor has not specified a default size.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        protected string GetDefaultDisplayOption(IContent content)
        {
            IDefaultDisplayOption contentWithDefaults = content as IDefaultDisplayOption;
            if(contentWithDefaults != null)
            {
                return contentWithDefaults.Tag;
            }

            // Check system types (where we do not control the model)
            if (content is ContentFolder)
            {
                return WebGlobal.ContentAreaTags.FullWidth;
            }

            return null;
        }

        /// <summary>
        /// Gets a CSS class used for styling based on a tag name (ie a Bootstrap class name)
        /// </summary>
        /// <param name="tagName">Any tag name available, see <see cref="WebGlobal.ContentAreaTags"/></param>
        private static string GetCssClassForTag(string tagName)
        {
            string tag = tagName;
            if(tag != null)
            {
                tag = tag.ToLower();
            }
            else
            {
                tag = "";
            }

            switch (tag)
            {
				case WebGlobal.ContentAreaTags.FullWidth:
                    return "full";
				case WebGlobal.ContentAreaTags.TwoThirdsWidth:
                    return "wide";
				case WebGlobal.ContentAreaTags.HalfWidth:
                    return "half";
				case WebGlobal.ContentAreaTags.OneThirdWidth:
					return "narrow";
				case WebGlobal.ContentAreaTags.Slider:
					return "slider";
				default:
                    return "narrow";
            }
        }

        private static string GetTypeSpecificCssClasses(IContent content)
        {
            var cssClass = content == null ? String.Empty : content.GetOriginalType().Name.ToLowerInvariant();

            var customClassContent = content as ICustomCssInContentArea;
            if (customClassContent != null && !string.IsNullOrWhiteSpace(customClassContent.ContentAreaCssClass))
            {
                cssClass += string.Format("{0}", customClassContent.ContentAreaCssClass);
            }
            return cssClass;
        }
    }
}

/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.DataAbstraction;
using EPiServer.Web;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;

namespace OxxCommerceStarterKit.Web.Business.Rendering
{
    public class SliderItemSettings
    {
        public SliderItemSettings()
        {

        }

        public SliderItemSettings(int delay, string cssClass)
        {
            Delay = delay;
            CssClass = cssClass;
        }

        public int Delay { get; set; }
        public string CssClass { get; set; }
    }

    /// <summary>
    /// Extends the default <see cref="ContentAreaRenderer"/> to apply custom CSS classes to each <see cref="ContentFragment"/>.
    /// </summary>
    public class SliderContentAreaRenderer : ContentAreaRenderer
    {
        private IContentRepository _contentRepository;
        private IContentRenderer _contentRenderer;

        public SliderContentAreaRenderer(IContentRenderer contentRenderer, IContentRepository contentRepository)
        {
            _contentRenderer = contentRenderer;
            _contentRepository = contentRepository;
        }

        public override void Render(HtmlHelper htmlHelper, ContentArea contentArea)
        {
            RenderWithoutContainer(htmlHelper, contentArea);
        }

        /// <summary>
        /// The container breaks the slider content, and we do not need it,
        /// since we won't edit the content when using this renderer
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="contentArea"></param>
        public virtual void RenderWithoutContainer(HtmlHelper htmlHelper, ContentArea contentArea)
        {
            if ((contentArea != null) && !contentArea.IsEmpty)
            {
                RenderContentAreaItems(htmlHelper, contentArea.FilteredItems);
            }
        }

        protected virtual bool GetViewDataSetting(HtmlHelper htmlHelper, string viewDataKey)
        {
            bool? nullable = (bool?)htmlHelper.ViewContext.ViewData[viewDataKey];
            if (nullable.HasValue)
            {
                return nullable.Value;
            }
            return false;
        }

        protected override void RenderContentAreaItem(HtmlHelper htmlHelper, ContentAreaItem contentAreaItem, string templateTag, string htmlTag,
            string cssClass)
        {
            ViewContext viewContext = htmlHelper.ViewContext;
            IContent content = contentAreaItem.GetContent();
            if (content != null)
            {
                using (new ContentAreaContext(viewContext.RequestContext, content.ContentLink))
                {
                    TemplateModel templateModel = this.ResolveTemplate(htmlHelper, content, templateTag);
                    if ((templateModel != null) || this.IsInEditMode(htmlHelper))
                    {
                        TagBuilder tagBuilder = new TagBuilder(htmlTag);

                        SliderItemSettings itemSettings = viewContext.ViewData["sliderSettings"] as SliderItemSettings;
                        MergeSettings(tagBuilder, itemSettings);
                        if (contentAreaItem.RenderSettings.ContainsKey("sliderSettings"))
                        {
                            itemSettings = contentAreaItem.RenderSettings["sliderSettings"] as SliderItemSettings;
                            MergeSettings(tagBuilder, itemSettings);
                        }
                        // We can override settings per content item
                        tagBuilder.MergeAttributes(contentAreaItem.RenderSettings);

                        BeforeRenderContentAreaItemStartTag(tagBuilder, contentAreaItem);
                        viewContext.Writer.Write(tagBuilder.ToString(TagRenderMode.StartTag));
                        
                        htmlHelper.RenderContentData(content, true, templateModel, _contentRenderer);

                        viewContext.Writer.Write(tagBuilder.ToString(TagRenderMode.EndTag));
                    }
                }
            }
            

        }

        protected virtual void MergeSettings(TagBuilder tagBuilder, SliderItemSettings sliderItemSettings)
        {
            if (sliderItemSettings != null)
            {
                tagBuilder.MergeAttribute("class", sliderItemSettings.CssClass);
                tagBuilder.Attributes["data-delay"] = sliderItemSettings.Delay.ToString();
            }
        }
    }
}

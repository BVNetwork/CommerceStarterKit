/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Web;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Mvc;
using OxxCommerceStarterKit.Web.Controllers;
using OxxCommerceStarterKit.Web.Models.Blocks;
using OxxCommerceStarterKit.Web.Models.PageTypes;

namespace OxxCommerceStarterKit.Web.Business.Rendering
{
    [ServiceConfiguration(typeof(IViewTemplateModelRegistrator))]
    public class TemplateCoordinator : IViewTemplateModelRegistrator
    {
        public const string BlockFolder = "~/Views/Shared/Blocks/";
        public const string PagePartialsFolder = "~/Views/Shared/PagePartials/";
        public const string FormFolder = "~/Views/Shared/Forms/";


        public static void OnTemplateResolved(object sender, TemplateResolverEventArgs args)
		{
			// Disable DefaultPageController for page types that shouldn't have any renderer as pages
			if (args.ItemToRender is IContainerPage && args.SelectedTemplate != null && args.SelectedTemplate.TemplateType == typeof(DefaultPageController))
			{
				args.SelectedTemplate = null;
			}
		}

        /// <summary>
        /// Registers renderers/templates which are not automatically discovered, 
        /// i.e. partial views whose names does not match a content type's name.
        /// </summary>
        /// <remarks>
        /// Using only partial views instead of controllers for blocks and page partials
        /// has performance benefits as they will only require calls to RenderPartial instead of
        /// RenderAction for controllers.
        /// Registering partial views as templates this way also enables specifying tags and 
        /// that a template supports all types inheriting from the content type/model type.
        /// </remarks>
		public void Register(TemplateModelCollection viewTemplateModelRegistrator)
		{

            // We want a special renderer for the HtmlBlock when rendered in full view
            viewTemplateModelRegistrator.Add(typeof (HtmlBlock), new TemplateModel()
            {
                Name = "Html Block rendered in Full view",
                Inherit = true,
                Tags = new[] { WebGlobal.ContentAreaTags.FullWidth },
                Path = BlockPath("HtmlBlock.Full.cshtml"),
                TemplateTypeCategory = TemplateTypeCategories.MvcPartialView,
                AvailableWithoutTag = false,
                Default = false
            });

            viewTemplateModelRegistrator.Add(typeof(ArticlePage), new TemplateModel()
            {
                Name = "Article rendered in Full view",
                Inherit = true,
                Tags = new[] { WebGlobal.ContentAreaTags.FullWidth },
                Path = BlockPath("ArticlePage.Full.cshtml"),
                TemplateTypeCategory = TemplateTypeCategories.MvcPartialView,
                AvailableWithoutTag = false,
                Default = false
            });

            viewTemplateModelRegistrator.Add(typeof(ArticleWithSidebarPage), new TemplateModel()
            {
                Name = "Article with sidebar rendered in Full view",
                Inherit = true,
                Tags = new[] { WebGlobal.ContentAreaTags.FullWidth },
                Path = BlockPath("ArticlePage.Full.cshtml"),
                TemplateTypeCategory = TemplateTypeCategories.MvcPartialView,
                AvailableWithoutTag = false,
                Default = false
            });


            // All Slider blocks are rendered specially
            viewTemplateModelRegistrator.Add(typeof(VariationContent), new TemplateModel()
            {
                Name = "Product Variation renderer for the Slider block",
                Inherit = true,
                Tags = new[] { WebGlobal.ContentAreaTags.Slider },
                Path = BlockPath("Slider.Variation.cshtml"),
                TemplateTypeCategory = TemplateTypeCategories.MvcPartialView,
                AvailableWithoutTag = false,
                Default = false
            });
            viewTemplateModelRegistrator.Add(typeof(ProductContent), new TemplateModel()
            {
                Name = "Product Variation renderer for the Slider block",
                Inherit = true,
                Tags = new[] { WebGlobal.ContentAreaTags.Slider },
                Path = BlockPath("Slider.Product.cshtml"),
                TemplateTypeCategory = TemplateTypeCategories.MvcPartialView,
                AvailableWithoutTag = false,
                Default = false
            });

            // Forms
            viewTemplateModelRegistrator.Add(typeof(EPiServer.Forms.Implementation.Elements.SubmitButtonElementBlock), new TemplateModel
            {
                Name = "SubmitFormElementBlock",
                Inherit = false,
                Default = true,
                AvailableWithoutTag = true,
                Path = FormPath("SubmitButtonElementBlock.ascx")
            });

            viewTemplateModelRegistrator.Add(typeof(EPiServer.Forms.Implementation.Elements.ChoiceElementBlock), new TemplateModel
            {
                Name = "ChoiceFormElementBlock",
                Inherit = true,
                Default = true,
                AvailableWithoutTag = true,
                Path = FormPath("ChoiceElementBlock.ascx")
            });

            viewTemplateModelRegistrator.Add(typeof(EPiServer.Forms.Implementation.Elements.NumberElementBlock), new TemplateModel
            {
                Name = "NumberFormElementBlock",
                Inherit = true,
                Default = true,
                AvailableWithoutTag = true,
                Path = FormPath("NumberElementBlock.ascx")
            });

            viewTemplateModelRegistrator.Add(typeof(EPiServer.Forms.Implementation.Elements.SelectionElementBlock), new TemplateModel
            {
                Name = "SelectionFormElementBlock",
                Inherit = true,
                Default = true,
                AvailableWithoutTag = true,
                Path = FormPath("SelectionElementBlock.ascx")
            });

            viewTemplateModelRegistrator.Add(typeof(EPiServer.Forms.Implementation.Elements.ParagraphTextElementBlock), new TemplateModel
            {
                Name = "ParagraphTextFormElementBlock",
                Inherit = true,
                Default = true,
                AvailableWithoutTag = true,
                Path = FormPath("ParagraphTextElementBlock.cshtml")
            });


            viewTemplateModelRegistrator.Add(typeof(EPiServer.Forms.Implementation.Elements.TextareaElementBlock), new TemplateModel
            {
                Name = "TextareaFormElementBlock",
                Inherit = true,
                Default = true,
                AvailableWithoutTag = true,
                Path = FormPath("TextareaElementBlock.ascx")
            });



            viewTemplateModelRegistrator.Add(typeof(EPiServer.Forms.Implementation.Elements.TextboxElementBlock), new TemplateModel
            {
                Name = "TextBoxFormElementBlock",
                Inherit = true,
                Default = true,
                AvailableWithoutTag = true,
                Path = FormPath("TextBoxElementBlock.ascx")
            });


        }

        public static string BlockPath(string fileName)
        {
            return string.Format("{0}{1}", BlockFolder, fileName);
        }

		public static string PagePartialPath(string fileName)
        {
            return string.Format("{0}{1}", PagePartialsFolder, fileName);
        }

        private static string FormPath(string fileName)
        {
            return string.Format("{0}{1}", FormFolder, fileName);
        }

    }
}

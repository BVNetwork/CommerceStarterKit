/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using EPiServer.Globalization;
using EPiServer.Shell.ObjectEditing;

namespace OxxCommerceStarterKit.Web.EditorDescriptors.SelectionFactories
{
    public class ColorSelectionFactory : ISelectionFactory
    {
        public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            var allColors = Enum.GetValues(typeof(ProductColor));
            ISelectItem[] colorItems = new ISelectItem[allColors.Length-1];
            for (int i = 1; i < allColors.Length; i++)
            {
                var value =
                    EPiServer.Framework.Localization.LocalizationService.Current.GetStringByCulture("/common/product/colors/" +
                                                                                                    (ProductColor)i, ContentLanguage.PreferredCulture);
                colorItems[i-1] = new SelectItem() { Text = value, Value = value };
            }
            return colorItems;
        }
    }

    public class RecommendationsModeSelectionFactory : ISelectionFactory
    {
        public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            var allModes = Enum.GetValues(typeof(RecommendationsMode));
            ISelectItem[] colorItems = new ISelectItem[allModes.Length];
            for (int i = 0; i < allModes.Length; i++)
            {
                var value =
                    EPiServer.Framework.Localization.LocalizationService.Current.GetStringByCulture("/common/recommendations/mode/" +
                                                                                                    (RecommendationsMode)i, ContentLanguage.PreferredCulture);
                colorItems[i] = new SelectItem() { Text = value, Value = value };
            }
            return colorItems;
        }
    }
}

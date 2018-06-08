using System;
using System.Collections.Generic;
using EPiServer.Globalization;
using EPiServer.Shell.ObjectEditing;

namespace OxxCommerceStarterKit.Web.EditorDescriptors.SelectionFactories
{
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
using System.Collections.Generic;
using EPiServer.Shell.ObjectEditing;

namespace OxxCommerceStarterKit.Web.EditorDescriptors.SelectionFactories
{
    public class ButtonColorSelectionFactory : GenericSelectionFactory
    {
        public override IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            return GetSelectionFromDictionary(
                new Dictionary<string, string>()
                {
                    {"Default", ""}, 
                    {"Red", "btn-danger"}, 
                    {"Yellow", "btn-warning"},
                    {"Green", "btn-success"}
                });
        }
    }
}
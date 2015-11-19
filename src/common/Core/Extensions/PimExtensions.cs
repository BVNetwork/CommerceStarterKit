using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using EPiServer.Commerce.Catalog.ContentTypes;

namespace OxxCommerceStarterKit.Core.Extensions
{
    public static class PimExtensions
    {
        public static int GetEntityId(this EntryContentBase entryContent)
        {
            if(entryContent["inRiverEntityId"] != null)
            {
                return (int) entryContent["inRiverEntityId"];
            }
            return 0;
        }

    }
}

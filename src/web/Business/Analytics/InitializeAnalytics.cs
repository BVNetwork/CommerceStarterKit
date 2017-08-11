/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.GoogleAnalytics.Web.Tracking;
using EPiServer.ServiceLocation;

namespace OxxCommerceStarterKit.Web.Business.Analytics
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class InitializeAnalytics : IConfigurableModule
    {
        public void Initialize(InitializationEngine context)
        {
        }

        public void Preload(string[] parameters) { }

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.StructureMap().Configure(c => c.For<UniversalSyntax>().Use<UniversalSyntaxEx>());
            
            // As the GA does only support one IPluginScript at the moment, we
            // override it here.
            context.StructureMap().Configure(c => c.For<IPluginScript>().Use<RequireEnhancedCommercePlugin>());
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}

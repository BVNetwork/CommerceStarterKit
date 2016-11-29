using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;

namespace OxxCommerceStarterKit.Web.Business.Initialization
{
    [InitializableModule]
    public class RemoveServerHeaderInitialization : IInitializableHttpModule
    {
        
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
        }

        public void Initialize(InitializationEngine context)
        {
            
        }

        public void Uninitialize(InitializationEngine context)
        {
            
        }

        public void InitializeHttpEvents(HttpApplication application)
        {
            application.BeginRequest += (sender, args) =>
            {
                HttpApplication app = sender as HttpApplication;
                if (app != null && app.Context != null)
                {
                    app.Context.Response.Headers.Remove("Server");
                }
            };
        }
    }
}
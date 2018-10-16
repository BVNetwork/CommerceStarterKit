using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.ConnectForCampaign.Core.Configuration;
using EPiServer.ConnectForCampaign.Core.Implementation;
using EPiServer.ConnectForCampaign.Services;
using EPiServer.ConnectForCampaign.Services.Implementation;
using EPiServer.Shell.ObjectEditing;


namespace OxxCommerceStarterKit.Web.Business
{
    public class CustomOptinProcessServive : OptinProcessService
    {
        public CustomOptinProcessServive(IServiceClientFactory serviceClientFactory, ICacheService cacheService,
            ICampaignSettings campaignSettings, IAuthenticationService authenticationService) : base(
            serviceClientFactory, cacheService, campaignSettings, authenticationService)
        {
        }

        public override IEnumerable<SelectItem> GetAllowedOptInProcesses()
        {
            // GetAllOptInProcesses() returns a list of all opt-in processes where each item is a Tuple<long, string, string>:
            // + Item1 is the Id of the optin process
            // + Item2 is the Name of the optin process
            // + Item3 is the Type of the optin process
            return GetAllOptInProcesses().Select(x => new SelectItem() {Text = x.Item2, Value = x.Item1});
        }


    }
}
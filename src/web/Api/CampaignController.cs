using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Newtonsoft.Json.Linq;
using OxxCommerceStarterKit.Web.Services;

namespace OxxCommerceStarterKit.Web.Api
{
    public class CampaignController : BaseApiController
    {

        private Injected<ILogger> _logger;
        private Injected<IEspService> _espService;
      
        [HttpPost]
        public async Task<string> WebhookSubscribe([FromBody]JToken jsonbody)
        {
            var returnValue = string.Empty;

            if(_logger.Service.IsInformationEnabled())
                _logger.Service.Log(Level.Information, "WebhookSubscribe");

            var dataset = jsonbody.ToObject<EmailResult>();

            if (dataset != null)
            {
                var email = dataset.Email;

                await _espService.Service.Subscribe(email, null);
            }

            return returnValue;
        }

        internal class EmailResult
        {
            public string Email { get; set; }
        }
    }
}
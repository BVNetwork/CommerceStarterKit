using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using EPiServer.Logging;

namespace OxxCommerceStarterKit.Web.Services
{
    public class CampaignEspService : IEspService
    {

        private readonly ILogger _logger = LogManager.GetLogger();

        public async Task<string> Subscribe(string email, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            string returnValue = string.Empty;

            if (!string.IsNullOrWhiteSpace(email))
            {
                var encodedEmail = HttpUtility.UrlEncode(email);

                var url = GetUrl(encodedEmail);

                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        url = string.Format("{0}&{1}={2}", url, parameter.Key, HttpUtility.UrlEncode(parameter.Value));
                    }
                }

                using (var client = new System.Net.Http.HttpClient())
                {
                    using (var response = await client.GetAsync(new Uri(url)))
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        if (_logger.IsInformationEnabled())
                            _logger.Log(Level.Information, "Email: " + email + " Result:" + content);

                        returnValue = content;
                    }
                }
            }

            return returnValue;
        }

        private static string GetUrl(string email)
        {

            var key = ConfigurationManager.AppSettings["CampaignKey"];
            var bmOptInId = ConfigurationManager.AppSettings["CampaignOptInId"];
            var bmOptinSource = ConfigurationManager.AppSettings["CampaignOptInSource"];

            return string.Format("https://api.broadmail.de/http/form/{0}/subscribe?bmOptInId={1}&bmRecipientId={2}&bmOptinSource={3}", key, bmOptInId, email, bmOptinSource);
        }
    }
}
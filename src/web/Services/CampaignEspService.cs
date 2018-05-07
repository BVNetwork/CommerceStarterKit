using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using EPiServer.ConnectForCampaign.Core.Configuration;
using EPiServer.ConnectForCampaign.Services;
using EPiServer.ConnectForCampaign.Services.Implementation;
using EPiServer.ConnectForCampaign.Services.Recipient;
using EPiServer.Logging;

namespace OxxCommerceStarterKit.Web.Services
{
    public class CampaignEspService : IEspService
    {
        private readonly IServiceClientFactory _serviceClientFactory;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICampaignSettings _iCampaignSettings;

        public CampaignEspService(IServiceClientFactory serviceClientFactory, IAuthenticationService authenticationService, ICampaignSettings iCampaignSettings)
        {
            _serviceClientFactory = serviceClientFactory;
            _authenticationService = authenticationService;
            _iCampaignSettings = iCampaignSettings;

            if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["CampaignRecipientListId"]))
            {
                _recipientListId = long.Parse(ConfigurationManager.AppSettings["CampaignRecipientListId"]);
            }
            else
            {
                _recipientListId = 0;
            }
        }

        private readonly ILogger _logger = LogManager.GetLogger(typeof(CampaignEspService));
 
        private readonly string _bmOptInId = ConfigurationManager.AppSettings["CampaignOptInId"];
        private readonly string _bmOptinSource = ConfigurationManager.AppSettings["CampaignOptInSource"];
        private readonly string _httpApiBaseUrl = ConfigurationManager.AppSettings["CampaignHttpBaseUrl"];
        private readonly long _recipientListId; 

        public async Task<string> Subscribe(string email, object values)
        {
            var queryString = ToQueryString(values);

            var encodedEmail = HttpUtility.UrlEncode(email);
 
            var url = string.Format("{0}/subscribe?bmOptInId={1}&bmRecipientId={2}&bmOptinSource={3}&bmOverwrite=true&{4}", 
                _httpApiBaseUrl,
                _bmOptInId, 
                encodedEmail, 
                _bmOptinSource,
                queryString
                ); 
 
            return await GetAsync(url);
        }


        public async Task<string> SubscribeOrRemove(string email, object values)
        {

            var subscribe = PropertyHasValue("interests", values);

            if (IsRecipientExisting(_recipientListId, email) && !subscribe)
            {
                Remove(email);
            }
            else if (subscribe)
            {
                return await Subscribe(email, values);
            }

            return null;
        }

        private static bool PropertyHasValue(string propertyName, object values)
        {
            var type = values.GetType();
            var props = type.GetProperties();

            var subscribe = props.Any(x =>
                x.Name == propertyName && !string.IsNullOrWhiteSpace(x.GetValue(values, null) as string));

            return subscribe;
        }

        private static string ToQueryString(object values)
        {
            var type = values.GetType();
            var props = type.GetProperties();
            var pairs = props.Select(x => x.Name + "=" +  GetUrlEncodedValue(values, x)).ToArray();
            return string.Join("&", pairs);
        }
 
        private static string GetUrlEncodedValue(object a, PropertyInfo x)
        {
            return x.GetValue(a, null) is string str ? HttpUtility.UrlEncode(str) : string.Empty;
        }
 
 
        private async Task<string> GetAsync(string url)
        {
            string returnValue;
            using (var client = new HttpClient())
            {
                var uri = new Uri(url);
                using (var response = await client.GetAsync(uri))
                {
                    var content = await response.Content.ReadAsStringAsync();
 
                    if (_logger.IsInformationEnabled())
                        _logger.Log(Level.Information, "Result:" + content);
 
                    returnValue = content;
                }
            }
 
            return returnValue;
        }
    

        public string GetNewsletterOptions(string email)
        {
            var sessionId = GetSessionToken();


            var client = _serviceClientFactory.GetRecipientClient();
            var exists = client.contains(sessionId, _recipientListId, email);
 
            if (exists)
            {
                    var getAttributesRequest = new getAttributesRequest(sessionId, _recipientListId, email, new[] { "interests" });
                    
                    var recipient = client.getAttributes(getAttributesRequest);
 
                    return recipient.getAttributesReturn[0];
            }
 
            return string.Empty;
        }

        public void Remove(string userId)
        {
            if (!IsRecipientExisting(_recipientListId, userId))
                return;

            try
            {
                _serviceClientFactory.GetRecipientClient().remove(GetSessionToken(), _recipientListId, userId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);                
            }
        }

        private bool IsRecipientExisting(long recipientListId, string userId)
        {
            try
            {
                return _serviceClientFactory.GetRecipientClient().contains(GetSessionToken(), recipientListId, userId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return false;
            }
        }

        private string GetSessionToken()
        {
            return _authenticationService.GetToken(_iCampaignSettings.MandatorId, _iCampaignSettings.UserName, _iCampaignSettings.Password, true);
        }
    }
}
using System;
using System.Web;
using System.Web.Helpers;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using OxxCommerceStarterKit.Core.Models;

namespace OxxCommerceStarterKit.Core.Services
{
    [ServiceConfiguration(typeof(ICookieService))]
    public class CookieService : ICookieService
    {
        public QuickBuyModel GetFromCookie()
        {
            try
            {
                var cookie = HttpContext.Current.Request.Cookies["QuickBuy"];
                var model = cookie == null
                    ? new QuickBuyModel()
                    : QuickBuyModel.FromString(HttpUtility.UrlDecode(cookie.Value));
                return model;
            }
            catch
            {
                return new QuickBuyModel();
            }
        }

        public void SaveCookie(QuickBuyModel model)
        {
            var cookie = HttpContext.Current.Request.Cookies["QuickBuy"] ?? new HttpCookie("QuickBuy");
            cookie.Expires = DateTime.Now.AddDays(1d);
            cookie.HttpOnly = true;
            cookie.Value = HttpUtility.UrlEncode(model.ToString());
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

     
    }
} 

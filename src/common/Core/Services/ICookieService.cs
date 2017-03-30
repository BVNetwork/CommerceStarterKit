using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxxCommerceStarterKit.Core.Models;

namespace OxxCommerceStarterKit.Core.Services
{
    public interface ICookieService
    {
        QuickBuyModel GetFromCookie();

        void SaveCookie(QuickBuyModel model);
    }
}

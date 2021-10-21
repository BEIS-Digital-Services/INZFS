
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.Theme
{
    /// <summary>
    /// this code must not go live as it will be an issue with CSRF for anonymous forms. this fix should take as a spike only
    /// </summary>
    public class ThisCodeMustNotGoLiveAntiforgery : IAntiforgery
    {
        public AntiforgeryTokenSet GetAndStoreTokens(HttpContext httpContext)
        {
            //HACK:Dummy tocken for form and cookies
            return new AntiforgeryTokenSet("token", "token", "tovalidate", "header");
        }

        public AntiforgeryTokenSet GetTokens(HttpContext httpContext)
        {
            //HACK:Dummy tocken for form and cookies
            return new AntiforgeryTokenSet("token", "token", "tovalidate", "header");
        }

        public async Task<bool> IsRequestValidAsync(HttpContext httpContext)
        {
            return true;
        }

        public void SetCookieTokenAndHeader(HttpContext httpContext)
        {

        }

        public async Task ValidateRequestAsync(HttpContext httpContext)
        {

        }
    }
}

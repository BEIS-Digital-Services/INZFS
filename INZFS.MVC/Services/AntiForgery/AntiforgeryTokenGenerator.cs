using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace INZFS.MVC.Services.AntiForgery
{
    public class AntiforgeryTokenGenerator : IAntiforgeryTokenGenerator
    {
        private readonly IAntiforgery _antiForgery;
        private readonly IHttpContextAccessor _context;

        public AntiforgeryTokenGenerator(IAntiforgery antiforgery, IHttpContextAccessor context)
        {
            _antiForgery = antiforgery;
            _context = context;
        }
        public string GenerateAntiforgeryToken()
        {
          
            string tokenValue = _antiForgery.GetAndStoreTokens(_context.HttpContext).RequestToken;
            return tokenValue;
        }

        public void SetAntiforgeryResponseHeader()
        {
            _context.HttpContext.Response.Headers.Add("RequestVerificationToken",GenerateAntiforgeryToken());
        }
    }
}

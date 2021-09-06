using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INZFS.Web.Middleware
{
    public class SecurityHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            context.Response.Headers.Add("referrer-policy", new StringValues("strict-origin-when-cross-origin"));

            context.Response.Headers.Add("x-content-type-options", new StringValues("nosniff"));

            context.Response.Headers.Add("x-frame-options", new StringValues("DENY"));

            context.Response.Headers.Add("X-Permitted-Cross-Domain-Policies", new StringValues("none"));

            context.Response.Headers.Add("x-xss-protection", new StringValues("1; mode=block"));

            context.Response.Headers.Add("frame-ancestors", new StringValues("none"));


            return _next(context);
        }
    }

}

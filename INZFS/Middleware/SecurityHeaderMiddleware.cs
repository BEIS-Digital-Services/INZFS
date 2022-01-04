using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
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
            string url = context.Request.Path.Value.ToLower();
            bool isAdmin = url.StartsWith("/admin/");
          
            context.Response.Headers.Add("referrer-policy", new StringValues("strict-origin-when-cross-origin"));

            context.Response.Headers.Add("x-content-type-options", new StringValues("nosniff"));

            context.Response.Headers.Add("x-frame-options", new StringValues("DENY"));

            context.Response.Headers.Add("X-Permitted-Cross-Domain-Policies", new StringValues("none"));

            context.Response.Headers.Add("x-xss-protection", new StringValues("1; mode=block"));

            context.Response.Headers.Add("frame-ancestors", new StringValues("none"));

            context.Response.Headers.Add("Content-Security-Policy", new StringValues(
                "base-uri 'self';" +
                "font-src 'self' https://www.gov.uk/assets/static/fonts/;" +
                "frame-ancestors 'none';" +
                "frame-src 'none';" +
                "img-src 'self' data: http://www.w3.org/2000/svg https://www.zcloud.net https://www.google-analytics.com/ otpauth://totp ;" +
                "media-src 'self';" +
                "object-src 'self';" +
                "script-src 'self' " +
                    (isAdmin ? "'unsafe-inline' 'unsafe-hashes' 'unsafe-eval' " :
                    "'sha256-E041E97A2AEEDAB8A6BB8C125A11C5F399004569D3681967DCEAE852F5D26EB1' " +
                    "'sha256-5BF7094BFF6D37EFF5AB6973DAADE41133478BC82216EA8CDB033D850C3EC4E1' " +
                    "'sha256-4D2E40B023AE7FA9BBC0DEDD1C9190954A63BA9BC851FA0F584A661579BA59ED' " +
                    "'sha256-TS5AsCOuf6m7wN7dHJGQlUpjupvIUfoPWEpmFXm6We0=' " +
                    "'sha256-shuRZfRwSWl228FDVUklziDxl8T7fgo7qpEkTq/IWt0=' " +
                    "'sha256-DAzDluGJFvFJyJvAnHTUT2PFSmds1nA7mX25IuziLVg=' " +
                    "'sha256-+2Jl1bBVk882OY6GArUVufVs87Iy+oVROeZngk4Yc2s=' " +
                    "'sha256-78J9Ge6YVlpe3T0Eb9DWGCLYR4HR1ZEoQ3IelgzT8wI=' ") +
                    "https://www.googletagmanager.com/gtag/ " +
                    "https://www.google-analytics.com/analytics.js " +
                    "https://ajax.aspnetcdn.com/ajax/jquery/jquery-3.6.0.min.js " +
                    "https://code.jquery.com/jquery-3.6.0.js " +
                    "https://design-system.service.gov.uk/javascripts/govuk-frontend-d7b7e40c8ac2bc81d184bb2e92d680b9.js; "
            )); ;

          return _next(context);
        }
    }

}

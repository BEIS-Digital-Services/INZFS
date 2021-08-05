using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace INZFS.Web.Middleware
{
    public class SetVcapSessionMiddleware
    {
        public IConfiguration Configuration { get; }
        public HttpContext _context { get; }

        private readonly RequestDelegate _next;
        public SetVcapSessionMiddleware(IConfiguration configuration, RequestDelegate next)
        {
            _next = next;
            Configuration = configuration;
        
        }
        public async Task InvokeAsync(HttpContext Context)
        {
            Context.Response.Cookies.Append("JSESSIONID", Context.Session.Id);
            await _next(Context);
        }
    }
}

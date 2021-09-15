using Microsoft.AspNetCore.Builder;

namespace INZFS.Web.Middleware
{
    public static class SetVcapSessionMiddlewareExtensions
    {
        public static IApplicationBuilder UseVcapSession(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SetVcapSessionMiddleware>();
        }
    }
}


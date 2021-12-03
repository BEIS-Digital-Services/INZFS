using Galebra.Security.Headers.Csp.Infrastructure;
using Microsoft.AspNetCore.Builder;
using System;

namespace Galebra.Security.Headers.Csp
{ 

public static class CspMiddlewareExtensions
{
    /// <summary>
    /// Injects the <see cref="CspMiddleware"/> into the pipeline to set the configured content security policies<br/>
    /// This middleware can be placed before or after UseStaticFiles.<br/>
    /// If placed before, a csp header will accompany every file delivered by wwwroot, such as favicon, svg, css, js, images etc
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseContentSecurityPolicy(this IApplicationBuilder app)
    {
        if (app is null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        return app.UseMiddleware<CspMiddleware>();
    }
}

}
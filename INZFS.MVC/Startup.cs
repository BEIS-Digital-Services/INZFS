using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace INZFS.MVC
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "BlogPost",
                areaName: "INZFS.MVC",
                pattern: "BlogPost/Index",
                defaults: new { controller = "BlogPost", action = "Index" }
            );
            routes.MapAreaControllerRoute(
                name: "FundApplication",
                areaName: "INZFS.MVC",
                pattern: "FundApplication/Index",
                defaults: new { controller = "FundApplication", action = "Index" }

                );

            routes.MapAreaControllerRoute(
            name: "ContactAndOrgDetails",
            areaName: "INZFS.MVC",
            pattern: "FundApplication/Index",
            defaults: new { controller = "FundApplication", action = "Index" }

            );

            routes.MapAreaControllerRoute(
                name: "FundApplication",
                areaName: "INZFS.MVC",
                pattern: "Application/Index",
                defaults: new { controller = "Application", action = "Index" }

                );

            routes.MapAreaControllerRoute(
                name: "FundApplication",
                areaName: "INZFS.MVC",
                pattern: "Application/Handle",
                defaults: new { controller = "Application", action = "Handle" }

                );
        }
    }
}

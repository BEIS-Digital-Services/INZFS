using System;
using INZFS.MVC.Drivers;
using INZFS.MVC.Handlers;
using INZFS.MVC.Migrations;
using INZFS.MVC.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;


namespace INZFS.MVC
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddContentPart<PersonPart>()
               .UseDisplayDriver<PersonPartDisplayDriver>()
               .AddHandler<PersonPartHandler>();
            services.AddScoped<IDataMigration, PersonMigration>();
            //services.AddContentPart<PersonPart>();
            //services.AddScoped<IDataMigration, PersonMigration>();
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
            pattern: "ContactAndOrgDetails/Index",
            defaults: new { controller = "ContactAndOrgDetails", action = "Index" }

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

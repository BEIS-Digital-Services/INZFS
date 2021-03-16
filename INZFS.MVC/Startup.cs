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

            services.AddContentPart<ProposalSummaryPart>()
               .UseDisplayDriver<ProposalSummaryPartDisplayDriver>();
            services.AddScoped<IDataMigration, ProposalSummaryMigration>();

            services.AddContentPart<ProjectSummaryPart>()
              .UseDisplayDriver<ProjectSummaryDriver>();
            services.AddScoped<IDataMigration, ProjectSummaryMigration>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {

            routes.MapAreaControllerRoute(
                name: "FundApplication",
                areaName: "INZFS.MVC",
                pattern: "{area:exists}/{controller=Home}/{action=section}/{pageName?}/{id?}"
            );
        }
    }
}

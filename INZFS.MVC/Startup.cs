using System;
using INZFS.MVC.Drivers;
using INZFS.MVC.Forms;

using INZFS.MVC.Migrations;
using INZFS.MVC.Models;
using INZFS.MVC.Services;
using INZFS.MVC.TagHelpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.TagHelpers;
using OrchardCore.Modules;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using System.IO;
using nClam;
using Microsoft.Extensions.Configuration;

namespace INZFS.MVC
{
    public class Startup : StartupBase
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ClamClient>(x =>
            {
                var host = Configuration["ClamAVServerHost"];
                if (int.TryParse(Configuration["ClamAVServerPort"], out var port))
                {
                    return new ClamClient(host, port);
                }
                else
                {
                    return new ClamClient(host);
                }
            });
            services.AddTagHelpers<AddClassTagHelper>();
            services.AddTagHelpers<ValidationMessageTagHelper>();
            services.AddTagHelpers<ValidationHighLighterTagHelper>();

            services.AddContentPart<ProjectSummaryPart>()
              .UseDisplayDriver<ProjectSummaryDriver>();
            services.AddScoped<IDataMigration, ProjectSummaryMigration>();

            services.AddContentPart<ProjectDetailsPart>()
            .UseDisplayDriver<ProjectDetailsDriver>();
            services.AddScoped<IDataMigration, ProjectDetailsMigration>();

            services.AddContentPart<OrgFundingPart>()
           .UseDisplayDriver<OrgFundingDriver>();
            services.AddScoped<IDataMigration, OrgFundingMigration>();

            services.AddScoped<INavigation, Navigation>();

            services.AddSingleton<IGovFileStore>(serviceProvider =>
            {

                var shellOptions = serviceProvider.GetRequiredService<IOptions<ShellOptions>>().Value;
                var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();

                var tenantFolderPath = PathExtensions.Combine(

                    shellOptions.ShellsApplicationDataPath,

                    shellOptions.ShellsContainerName,

                    shellSettings.Name);


                var customFolderPath = PathExtensions.Combine(tenantFolderPath, "CustomFiles");


                return new GovFileStore(customFolderPath);
            });

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

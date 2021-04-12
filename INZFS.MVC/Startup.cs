using System;
using INZFS.MVC.Drivers;
using INZFS.MVC.Drivers.ProposalFinance;
using INZFS.MVC.Drivers.ProposalWritten;
using INZFS.MVC.Forms;

using INZFS.MVC.Migrations;
using INZFS.MVC.Migrations.ProposalFinance;
using INZFS.MVC.Migrations.ProposalWritten;
using INZFS.MVC.Models;
using INZFS.MVC.Models.ProposalFinance;
using INZFS.MVC.Models.ProposalWritten;
using INZFS.MVC.TagHelpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.TagHelpers;
using OrchardCore.Modules;


namespace INZFS.MVC
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {

            services.AddTagHelpers<AddClassTagHelper>();
            services.AddTagHelpers<ValidationMessageTagHelper>();
            services.AddTagHelpers<ValidationHighLighterTagHelper>();

            ConfigureContent(services);

            services.AddScoped<INavigation, Navigation>();
        }

        private void ConfigureContent(IServiceCollection services)
        {
            services.AddContentPart<ProjectSummaryPart>()
            .UseDisplayDriver<ProjectSummaryDriver>();
            services.AddScoped<IDataMigration, ProjectSummaryMigration>();

            services.AddContentPart<ProjectDetailsPart>()
            .UseDisplayDriver<ProjectDetailsDriver>();
            services.AddScoped<IDataMigration, ProjectDetailsMigration>();

            services.AddContentPart<OrgFundingPart>()
           .UseDisplayDriver<OrgFundingDriver>();
            services.AddScoped<IDataMigration, OrgFundingMigration>();

            services.AddContentPart<ProjectProposalDetailsPart>()
            .UseDisplayDriver<ProjectProposalDetailsDriver>();
            services.AddScoped<IDataMigration, ProjectProposalDetailsMigration>();

            services.AddContentPart<ProjectExperiencePart>()
            .UseDisplayDriver<ProjectExperienceDriver>();
            services.AddScoped<IDataMigration, ProjectExperienceMigration>();

            services.AddContentPart<FinanceTurnoverPart>()
           .UseDisplayDriver<FinanceTurnoverDriver>();
            services.AddScoped<IDataMigration, FinanceTurnoverMigration>();
        }
        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
               name: "FundApplication",
               areaName: "INZFS.MVC",
               pattern: "{controller=Home}/{action=section}/{pageName?}/{id?}"
           );
        }
    }
}

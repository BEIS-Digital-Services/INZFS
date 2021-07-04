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
using INZFS.MVC.Handlers;
using INZFS.MVC.Navigations;
using OrchardCore.Navigation;
using System.Text.Json;
using System.Text.Json.Serialization;
using INZFS.MVC.ModelProviders;
using YesSql.Indexes;
using INZFS.MVC.Records;
using INZFS.MVC.Migrations.Indexes;
using INZFS.MVC.Services.FileUpload;
using INZFS.MVC.Services.VirusScan;
using Azure.Storage.Blobs;

namespace INZFS.MVC
{
    public class Startup : StartupBase
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public string AccessBlobstorage()
        {
            string connectionString = Configuration["AzureBlobStorage"];
            string containerName = "inzfs";
            string blobName = "INZFS.json";
            var blobToDownload = new BlobClient(connectionString, containerName, blobName).DownloadContent().Value;
            string jsonString = blobToDownload.Content.ToString();
            return jsonString;
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

            ConfigureContent(services);

            services.AddScoped<IContentRepository, ContentRepository>();
            services.AddScoped<INavigation, Navigation>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IFileUploadService, FileUploadService>();
            services.AddScoped<IVirusScanService, VirusScanService>();
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

            services.AddSingleton<ApplicationDefinition>(sp =>
            {
                string jsonString = AccessBlobstorage();
                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                options.Converters.Add(new JsonStringEnumConverter());
                var applicationDefinition = JsonSerializer.Deserialize<ApplicationDefinition>(jsonString, options);
                return applicationDefinition;
            });
            //services.AddControllers();
            services.AddControllers(options =>
            {
                options.ModelBinderProviders.Insert(0, new BaseModelBinderProvider());
            });
        }

        private void ConfigureContent(IServiceCollection services)
        {
            
            services.AddScoped<IDataMigration, ApplicationContentIndexMigration>();
            services.AddSingleton<IIndexProvider, ApplicationContentIndexProvider>();


            services.AddContentPart<CompanyDetailsPart>()
            .UseDisplayDriver<CompanyDetailsDriver>();
            services.AddScoped<IDataMigration, CompanyDetailsMigration>();

            services.AddContentPart<ProjectSummaryPart>()
            .UseDisplayDriver<ProjectSummaryDriver>()
            .AddHandler<ProjectSummaryPartHandler>();

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

            services.AddContentPart<FinanceBalanceSheetPart>()
           .UseDisplayDriver<FinanceBalanceSheetDriver>();
            services.AddScoped<IDataMigration, FinanceBalanceSheetMigration>();

            services.AddContentPart<FinanceRecoverVatPart>()
           .UseDisplayDriver<FinanceRecoverVatDriver>();
            services.AddScoped<IDataMigration, FinanceRecoverVatMigration>();

            services.AddContentPart<FinanceBarriersPart>()
           .UseDisplayDriver<FinanceBarriersDriver>();
            services.AddScoped<IDataMigration, FinanceBarriersMigration>();

            services.AddContentPart<ApplicationDocumentPart>()
                .UseDisplayDriver<ApplicationDocumentDriver>()
                  .AddHandler<ApplicationDocumentPartHandler>();
            services.AddScoped<IDataMigration, ApplicationDocumentMigration>();

            services.AddScoped<IDataMigration, ApplicationContainerMigration>();
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

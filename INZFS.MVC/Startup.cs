using System;
using INZFS.MVC.Forms;
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
using INZFS.MVC.Services.UserService;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Microsoft.OpenApi.Models;
using OrchardCore.Users;
using Microsoft.AspNetCore.Identity;
using Swashbuckle.AspNetCore.SwaggerUI;
using NetEscapades.AspNetCore.SecurityHeaders;
using System.Text;
using Microsoft.AspNetCore.Http;
using INZFS.MVC.Services.Swagger;

namespace INZFS.MVC
{
    public class Startup : StartupBase
    {
        private readonly IHostEnvironment _environment;
        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            _environment = environment;
        }

        public IConfiguration Configuration { get; }
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddOptions<SwaggerUIOptions>()
                .Configure<IHttpContextAccessor>((swaggerUiOptions, httpContextAccessor) =>
                {
            var originalIndexStreamFactory = swaggerUiOptions.IndexStream;

                    
                    swaggerUiOptions.IndexStream = () =>
                    {
                using var originalStream = originalIndexStreamFactory();
                        using var originalStreamReader = new StreamReader(originalStream);
                        var originalIndexHtmlContents = originalStreamReader.ReadToEnd();

                var requestSpecificNonce = httpContextAccessor.HttpContext.GetNonce();

                var nonceEnabledIndexHtmlContents = originalIndexHtmlContents
                            .Replace("<script>", $"<script nonce=\"{requestSpecificNonce}\">", StringComparison.OrdinalIgnoreCase)
                            .Replace("<style>", $"<style nonce=\"{requestSpecificNonce}\">", StringComparison.OrdinalIgnoreCase);

                return new MemoryStream(Encoding.UTF8.GetBytes(nonceEnabledIndexHtmlContents));
                    };
                });
            services.AddSwaggerGen(c =>
            {
                c.DocumentFilter<SwaggerFilter>();
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "List Applications",
                    Description = "Web API for updating application Status",
                    TermsOfService = new Uri("https://inzfs-sandbox.london.cloudapps.digital/"),
                    Contact = new OpenApiContact
                    {
                        Name = "Lorenzo Lane",
                        Email = "lorenzo.lane@beis.gov.uk",
                        Url = new Uri("https://inzfs-sandbox.london.cloudapps.digital/"),
                    }
                    
                });
            });
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
            services.AddScoped<INavigationProvider, Navigations.AdminMenu>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IFileUploadService, FileUploadService>();
            services.AddScoped<IVirusScanService, VirusScanService>();
            services.AddScoped<IApplicationNumberGenerator, ApplicationNumberGenerator>();
            services.AddScoped<IUserService, UserService>();
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
                string fileName = "INZFS.MVC.INZFS.json";
                Assembly assembly = Assembly.GetExecutingAssembly();
                var stream = assembly.GetManifestResourceStream(fileName);
                StreamReader reader = new StreamReader(stream);
                string jsonString = reader.ReadToEnd();
                reader.Close();
                stream.Close();

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

        }
        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            builder.UseSecurityHeaders(policyCollection =>
            {
                policyCollection.AddContentSecurityPolicy(csp =>
                {

                    csp.AddScriptSrc()
                       .Self()
                       .From("https://ajax.aspnetcdn.com/ajax/jquery/jquery-3.6.0.min.js")
                       .From("https://code.jquery.com/jquery-3.6.0.js")
                       .From("https://design-system.service.gov.uk/javascripts/")
                       .WithNonce();

        });
            });
            builder.UseSwagger();
            builder.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ListApplications v1"));
            routes.MapAreaControllerRoute(
               name: "FundApplication",
               areaName: "INZFS.MVC",
               pattern: "{controller=Home}/{action=section}/{pageName?}/{id?}"
           );
        }
    }
}

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
using Azure.Storage.Blobs;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Notify.Interfaces;
using Notify.Client;
using INZFS.MVC.Services.PdfServices;
using INZFS.MVC.Services.Zip;
using INZFS.MVC.Validators;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyModel;
using System.Linq;
using INZFS.MVC.Migrations;
using INZFS.MVC.Filters;
using INZFS.MVC.Settings;
using Microsoft.AspNetCore.Razor.TagHelpers;
using INZFS.MVC.Services.UserManager;
using INZFS.MVC.Services.AzureStorage;
using INZFS.MVC.Services.FundApplication;

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
            services.AddTagHelpers<AddClassTagHelper>();
            services.AddTagHelpers<ValidationMessageTagHelper>();
            services.AddTagHelpers<ValidationHighLighterTagHelper>();

            ConfigureContent(services);

            services.AddScoped<IContentRepository, ContentRepository>();
            services.AddScoped<INavigation, Navigation>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IFileUploadService, FileUploadService>();
            services.AddScoped<IZipService, ZipService>();
            services.AddScoped<IVirusScanService, VirusScanService>();
            services.AddScoped<IApplicationNumberGenerator, ApplicationNumberGenerator>();
            services.AddScoped<IUserManagerService, UserManagerService>();
            services.AddScoped<IAzureBlobService, AzureBlobService>();
            services.AddScoped<IApplicationGeneratorService, ApplicationGeneratorService>();
            services.AddScoped<IDynamicFormGenerator, DynamicFormGenerator>();
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
            services.AddScoped<INotificationClient>(services =>
                new NotificationClient(Configuration.GetValue<string>("GovNotifyApiKey")));
            services.AddScoped<IApplicationEmailService, ApplicationEmailService>();

            services.AddScoped<ICustomerValidatorFactory, CustomerValidatorFactory>();
            services.AddScoped<ApplicationRedirectionAttribute>();

            services.Configure<ApplicationOption>(Configuration.GetSection("Application"));
            services.Configure<GoogleAnalyticsOptions>(options => Configuration.GetSection("GoogleAnalytics").Bind(options));
            
            RegisterCustomValidators(services);
            services.AddHttpContextAccessor();
        }

        private static void RegisterCustomValidators(IServiceCollection serviceCollection)
        {
            var customValidators = GetAllTypesOf<ICustomValidator>();

            foreach (var customValidator in customValidators)
            {
                if (customValidator.IsInterface || customValidator.IsAbstract) continue;
                var serviceType = customValidator.GetInterfaces()[0];
                serviceCollection.AddScoped(serviceType, customValidator);
            }
        }

        private static IEnumerable<Type> GetAllTypesOf<T>()
        {
            var platform = Environment.OSVersion.Platform.ToString();
            var runtimeAssemblyNames = DependencyContext.Default.GetRuntimeAssemblyNames(platform);

            return runtimeAssemblyNames
                .Select(Assembly.Load)
                .SelectMany(a => a.ExportedTypes)
                .Where(t => typeof(T).IsAssignableFrom(t));
        }

        private void ConfigureContent(IServiceCollection services)
        {
            services.AddScoped<IDataMigration, ApplicationContentIndexMigration>();
            services.AddSingleton<IIndexProvider, ApplicationContentIndexProvider>();

            services.AddScoped<IDataMigration, ApplicationContentUserIdIndexMigration>();
            services.AddSingleton<IIndexProvider, ApplicationContentUserIdIndexProvider>(); 
            
            services.AddScoped<IDataMigration, ApplicationOutcomeStatusIdIndexMigration>();
            services.AddSingleton<IIndexProvider, ApplicationOutcomeStatusIdIndexProvider>();

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

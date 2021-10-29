using System;
using INZFS.Theme.Migrations;
using INZFS.Theme.Models;
using INZFS.Theme.Records;
using INZFS.Theme.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notify.Client;
using Notify.Interfaces;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
using OrchardCore.Users;
using YesSql.Indexes;

namespace INZFS.Theme
{
    public class Startup : StartupBase
    {
        private readonly ILogger<Startup> _logger;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            _logger = logger;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();

            serviceCollection.AddData();
            serviceCollection.AddAuthentications();
            serviceCollection.AddTwoFactorAuthentication(Configuration);

            serviceCollection.AddScoped<INotificationClient>(services =>
                new NotificationClient(Configuration.GetValue<string>("GovNotifyApiKey")));
         
            serviceCollection.AddScoped<IUrlEncodingService, UrlEncodingService>();
            serviceCollection.AddScoped<IRegistrationQuestionnaireService, RegistrationQuestionnaireService>();

            serviceCollection.AddKeyManagementOptions(Configuration, _logger);
            serviceCollection.AddSingleton<IAntiforgery, ThisCodeMustNotGoLiveAntiforgery>();

            serviceCollection.AddCommonPasswordCheck(Configuration);

        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "INZFSThemeDefault",
                areaName: "INZFS.Theme",
                pattern: "{controller=TwoFactorController}/{action=index}"
            );
        }
    }
}

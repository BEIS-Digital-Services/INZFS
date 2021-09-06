using System;
using INZFS.Theme.Migrations;
using INZFS.Theme.Models;
using INZFS.Theme.Records;
using INZFS.Theme.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public override void ConfigureServices(IServiceCollection serviceCollection)
        {

            serviceCollection.AddData();
            serviceCollection.AddAuthentications();
            serviceCollection.AddTwoFactorAuthentication(Configuration);

            serviceCollection.AddScoped<INotificationClient>(services =>
                new NotificationClient(Configuration.GetValue<string>("GovNotifyApiKey")));
         
            serviceCollection.AddScoped<IUrlEncodingService, UrlEncodingService>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "TwoFactor",
                areaName: "INZFS.Theme",
                pattern: "Account/{controller=TwoFactorController}/{action=index}"
            );
        }
    }
}

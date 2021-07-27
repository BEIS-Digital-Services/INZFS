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
            serviceCollection.AddScoped<IResourceManifestProvider, ResourceManifest>();
            serviceCollection.AddScoped<IDataMigration, UserTwoFactorSettingsIndexMigration>();
            serviceCollection.AddSingleton<IIndexProvider, UserTwoFactorSettingsIndexProvider>();

            serviceCollection.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredUniqueChars = 3;
                options.Password.RequiredLength = 8;
            });

            serviceCollection.AddScoped<ITwoFactorAuthenticationService, TwoFactorAuthenticationService>();
            serviceCollection.AddScoped<IUserStore<IUser>, UserTwoFactorStore>();
            serviceCollection.AddScoped<IUserTwoFactorSettingsService, UserTwoFactorSettingsService>();
            serviceCollection.AddScoped<IEmailService, EmailService>();

            serviceCollection.Configure<TwoFactorOption>(Configuration.GetSection("TwoFactor"));
            serviceCollection.AddScoped<INotificationClient>(services => new NotificationClient(Configuration.GetValue<string>("ApiKey")));

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

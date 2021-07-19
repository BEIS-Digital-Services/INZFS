using INZFS.Theme.Migrations;
using INZFS.Theme.Records;
using INZFS.Theme.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
using OrchardCore.Users;
using YesSql.Indexes;

namespace INZFS.Theme
{
    public class Startup : StartupBase
    {
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

        }
    }
}

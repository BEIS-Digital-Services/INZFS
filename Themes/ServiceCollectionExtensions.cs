using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using INZFS.Theme;
using INZFS.Theme.Migrations;
using INZFS.Theme.Models;
using INZFS.Theme.Records;
using INZFS.Theme.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.ResourceManagement;
using OrchardCore.Users;
using YesSql.Indexes;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddData(
            this IServiceCollection services)
        {
            services.AddScoped<IDataMigration, UserTwoFactorSettingsIndexMigration>();
            services.AddSingleton<IIndexProvider, UserTwoFactorSettingsIndexProvider>(); 
            
            services.AddScoped<IDataMigration, RegistrationQuestionnaireIndexMigration>();
            services.AddSingleton<IIndexProvider, RegistrationQuestionnaireIndexProvider>();

            return services;
        }
        
        public static IServiceCollection AddAuthentications(
            this IServiceCollection services)
        {
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/LogOff";
                options.AccessDeniedPath = "/Error/403";
            });

            services.Configure<IdentityOptions>(options =>
            {
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredUniqueChars = 3;
                options.Password.RequiredLength = 8;
            });

            services.AddAuthentication(RegistrationConstants.RegistrationCookie)
                .AddCookie(RegistrationConstants.RegistrationCookie);
            
            services.AddScoped<IRegistrationManager, RegistrationManager>();

            return services;
        }
        
        public static IServiceCollection AddTwoFactorAuthentication(
            this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddScoped<ITwoFactorAuthenticationService, TwoFactorAuthenticationService>();
            services.AddScoped<IUserStore<IUser>, UserTwoFactorStore>();
            services.AddScoped<IUserTwoFactorSettingsService, UserTwoFactorSettingsService>();
            services.AddScoped<INotificationService, NotificationService>();

            services.Configure<TwoFactorOption>(Configuration.GetSection("TwoFactor"));
            services.Configure<NotificationOption>(Configuration.GetSection("Notification"));

            return services;
        }
    }
}

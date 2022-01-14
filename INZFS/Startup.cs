using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using OrchardCore.Logging;
using Serilog;
using Prometheus;
using INZFS.Web.Middleware;
using Galebra.Security.Headers.Csp;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace INZFS
{
    public class Startup
    {
        private readonly IHostEnvironment _environment;
        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            _environment = environment;
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (_environment.IsDevelopment())
            {
                services.AddOrchardCms().AddSetupFeatures("OrchardCore.AutoSetup");

                services.AddDistributedMemoryCache();
            }
            else
            {
                services.AddOrchardCms().AddSetupFeatures("OrchardCore.Redis.Cache", "OrchardCore.Redis.Lock", "OrchardCore.AutoSetup").AddAzureShellsConfiguration();

                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = Environment.GetEnvironmentVariable("OrchardCore__OrchardCore_Redis__Configuration");
                    options.InstanceName = "EEF";
                });
            }
            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(EEFGoogleAnalyticsFilter));
            });
            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = 0;
                options.IdleTimeout = TimeSpan.FromMinutes(30);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseSession();
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always,
                MinimumSameSitePolicy = SameSiteMode.Strict,
                Secure = CookieSecurePolicy.Always
            });
            //app.UseVcapSession();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseMetricServer();
            app.UseHttpMetrics();
            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();
            app.UseContentSecurityPolicy();
            app.UseOrchardCore(c => c.UseSerilogTenantNameLogging());
        }
    }
}

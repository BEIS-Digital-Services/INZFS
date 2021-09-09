using INZFS.Web.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrchardCore.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prometheus;
using Microsoft.AspNetCore.Http;

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
            //app.UseMiddleware<SecurityHeaderMiddleware>();
            var policyCollection = new HeaderPolicyCollection()
              .AddFrameOptionsSameOrigin()
              .AddXssProtectionBlock()
              .AddContentTypeOptionsNoSniff()
              .AddReferrerPolicyStrictOriginWhenCrossOrigin()
              .RemoveServerHeader()
              .AddContentSecurityPolicy(builder =>
              {
                  builder.AddObjectSrc().Self();
                  builder.AddFormAction().Self();
                  builder.AddFrameAncestors().Self();
                  builder.AddDefaultSrc().Self();
                  builder.AddFontSrc().Self().From("cdn.jsdelivr.net").From("fonts.googleapis.com").From("fonts.gstatic.com");

                  builder.AddStyleSrc().UnsafeInline().Self().From("cdn.jsdelivr.net").From("fonts.googleapis.com").From("cdn.datatables.net")
                  .From("unpkg.com")
                  .From("cdnjs.cloudflare.com")
                  ;
                  // unsafe-evail needed for vue.js runtime templates
                  builder.AddScriptSrc().UnsafeEval().UnsafeInline().Self()
                   .From("cdn.jsdelivr.net").From("cdn.datatables.net").From("cdnjs.cloudflare.com").From("vuejs.org")
                   .From("unpkg.com")
                   .From("cdnjs.cloudflare.com")
                   .From("https://www.googletagmanager.com/gtag/")
                   .From("https://ajax.aspnetcdn.com/ajax/jquery/jquery-3.6.0.min.js")
                   .From("https://code.jquery.com/jquery-3.6.0.js")
                   .From("https://design-system.service.gov.uk/javascripts/govuk-frontend-d7b7e40c8ac2bc81d184bb2e92d680b9.js")

                   ;
              });
            app.UseSecurityHeaders(policyCollection)
                .UsePoweredByOrchardCore(false)
                .UseStaticFiles()
                .UseOrchardCore(builder => builder
                    .UsePoweredByOrchardCore(false)
                    .UseCookiePolicy(new CookiePolicyOptions()
                    {
                        HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always,
                        Secure = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest,
                        MinimumSameSitePolicy = SameSiteMode.Strict,
                    })
                );
            app.UseSession();
            app.UseVcapSession();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseMetricServer();
            app.UseHttpMetrics();
            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();
            app.UseOrchardCore(c => c.UseSerilogTenantNameLogging());
            }
        }
}

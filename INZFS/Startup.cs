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
            services.AddOrchardCms().AddSetupFeatures("OrchardCore.Redis.Lock","OrchardCore.AutoSetup").AddDatabaseShellsConfiguration();
            services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(20);
            }); 
                //options.Cookie.Name = "JSESSIONID";
                //options.Cookie.IsEssential = true;
            });
            if (_environment.IsDevelopment())
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = Configuration["MyRedisConStr"];
                    options.InstanceName = "EEF";
                });
            }
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

            app.UseStaticFiles();
            app.UseRouting();
            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();
            app.UseOrchardCore(c => c.UseSerilogTenantNameLogging());
        }
    }
}

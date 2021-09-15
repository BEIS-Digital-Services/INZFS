using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prometheus;

namespace INZFS
{
    public class Program
    {
        private static readonly Gauge _InfoGauge = 
            Metrics.CreateGauge("web_info", "Web app info", "dotnet_version", "assembly_name", "app_version");
        public static void Main(string[] args)
        {
            _InfoGauge.Labels("5.0", "INZFS.Web", "1.1.0").Set(1);
            CreateHostBuilder(args).Build().Run();      
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging => logging.ClearProviders())
                .UseSerilog((hostingContext, configBuilder) =>
                {
                    configBuilder.ReadFrom.Configuration(hostingContext.Configuration).Enrich.FromLogContext();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel((options) =>
                    {
                        options.AddServerHeader = false;
                    });
                });
    }
}

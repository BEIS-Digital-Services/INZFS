using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INZFS
{
    public class Program
    {
        public static void Main(string[] args)
        {
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
                });
    }
}

using System;
using INZFS.Workflows.Activities;
using INZFS.Workflows.Drivers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;
using Notify.Client;

namespace INZFS.Workflows
{
    public class Startup : StartupBase
    {
        
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddActivity<GovEmail, GovEmailDriver>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        { 
        }
    }
}

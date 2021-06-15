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
        private string apiKey = "lltestapi-bb94d8fd-a2ae-472a-b355-9c39d6d0b916-32fd33b5-e505-4bba-b304-d5cbfd3cdea0";
        
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddActivity<GovEmail, GovEmailDriver>();
            var client = new NotificationClient(apiKey);
            client.SendEmail(
                    emailAddress: "lorenzo.lane@beis.gov.uk",
                    templateId: "8ca9aa23-ecf9-4f57-b5f3-0d662d5e7237");
                            }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        { 
        }
    }
}

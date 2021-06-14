using INZFS.Templates.Drivers;
using INZFS.Templates.Fields;
using INZFS.Templates.Settings;
using INZFS.Templates.ViewModels;
using INZFS.Templates.Indexing;
using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Modules;
using OrchardCore.DisplayManagement.TagHelpers;
using OrchardCore.Indexing;

namespace INZFS.Templates
{
    public class Startup : StartupBase
    {
        public Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<CodeField>();
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayCodeFieldViewModel>();
            TemplateContext.GlobalMemberAccessStrategy.Register<GovTextField>();
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayGovTextFieldViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTagHelpers<AddClassTagHelper>();
            services.AddTagHelpers<ValidationMessageTagHelper>();
            services.AddContentField<CodeField>()
                .UseDisplayDriver<CodeFieldDisplayDriver>();

            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, CodeFieldSettingsDriver>();
            services.AddContentField<GovTextField>()
               .UseDisplayDriver<GovTextFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, GovTextFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, GovTextFieldIndexHandler>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, GovTextFieldPredefinedListEditorSettingsDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, GovTextFieldHeaderDisplaySettingsDriver>();
        }
    }
}
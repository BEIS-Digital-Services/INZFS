using System.Threading.Tasks;
using INZFS.Templates.Fields;
using INZFS.Templates.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace INZFS.Templates.Settings
{
    public class GovTextFieldHeaderDisplaySettingsDriver : ContentPartFieldDefinitionDisplayDriver<GovTextField>
    {
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<HeaderSettingsViewModel>("GovTextFieldHeaderDisplaySettings_Edit", model =>
            {
                var settings = partFieldDefinition.GetSettings<GovTextFieldHeaderDisplaySettings>();

                model.Level = settings.Level;
            })
            .Location("DisplayMode");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            if (partFieldDefinition.DisplayMode() == "Header")
            {
                var model = new HeaderSettingsViewModel();
                var settings = new GovTextFieldHeaderDisplaySettings();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                settings.Level = model.Level;

                context.Builder.WithSettings(settings);
            }

            return Edit(partFieldDefinition);
        }
    }
}
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using INZFS.Templates.Fields;
using INZFS.Templates.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace INZFS.Templates.Settings
{
    public class GovTextFieldPredefinedListEditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver<GovTextField>
    {
        private readonly IStringLocalizer S;

        public GovTextFieldPredefinedListEditorSettingsDriver(IStringLocalizer<GovTextFieldPredefinedListEditorSettingsDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<PredefinedListSettingsViewModel>("GovTextFieldPredefinedListEditorSettings_Edit", model =>
            {
                var settings = partFieldDefinition.GetSettings<GovTextFieldPredefinedListEditorSettings>();

                model.DefaultValue = settings.DefaultValue;
                model.Editor = settings.Editor;
                model.Options = JsonConvert.SerializeObject(settings.Options ?? new ListValueOption[0], Formatting.Indented);
            })
            .Location("Editor");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            if (partFieldDefinition.Editor() == "PredefinedList")
            {
                var model = new PredefinedListSettingsViewModel();
                var settings = new GovTextFieldPredefinedListEditorSettings();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                try
                {
                    settings.DefaultValue = model.DefaultValue;
                    settings.Editor = model.Editor;
                    settings.Options = string.IsNullOrWhiteSpace(model.Options)
                        ? new ListValueOption[0]
                        : JsonConvert.DeserializeObject<ListValueOption[]>(model.Options);
                }
                catch
                {
                    context.Updater.ModelState.AddModelError(Prefix, S["The options are written in an incorrect format."]);
                    return Edit(partFieldDefinition);
                }

                context.Builder.WithSettings(settings);
            }

            return Edit(partFieldDefinition);
        }
    }
}
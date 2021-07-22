using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using INZFS.Templates.Fields;
using INZFS.Templates.Settings;
using INZFS.Templates.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

namespace INZFS.Templates.Drivers
{
    public class GovTextFieldDisplayDriver : ContentFieldDisplayDriver<GovTextField>
    {
        private readonly IStringLocalizer S;

        public GovTextFieldDisplayDriver(IStringLocalizer<GovTextFieldDisplayDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Display(GovTextField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayGovTextFieldViewModel>(GetDisplayShapeType(context), model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Detail", "Content")
            .Location("Summary", "Content");
        }

        public override IDisplayResult Edit(GovTextField field, BuildFieldEditorContext context)
        {
            return Initialize<EditGovTextFieldViewModel>(GetEditorShapeType(context), model =>
            {
                model.Text = field.Text;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(GovTextField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            if (await updater.TryUpdateModelAsync(field, Prefix, f => f.Text))
            {
                var settings = context.PartFieldDefinition.GetSettings<GovTextFieldSettings>();
                if (settings.Required && String.IsNullOrWhiteSpace(field.Text))
                {
                    updater.ModelState.AddModelError(Prefix, nameof(field.Text), S["A value is required for {0}.", context.PartFieldDefinition.DisplayName()]);
                }
            }

            return Edit(field, context);
        }
    }
}
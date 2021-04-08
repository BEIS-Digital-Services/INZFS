using System;
using INZFS.MVC;
using INZFS.MVC.Models;
using INZFS.MVC.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System.Globalization;
using System.Threading.Tasks;

namespace INZFS.MVC.Drivers
{
    public class ProjectSummaryDriver : ContentPartDisplayDriver<ProjectSummaryPart>
    {
        public override IDisplayResult Display(ProjectSummaryPart part, BuildPartDisplayContext context) =>
            Initialize<ProjectSummaryViewModel>(GetDisplayShapeType(context), viewModel => PopulateViewModel(part, viewModel))
                .Location("Detail", "Content:1")
                .Location("Summary", "Content:1");

        public override IDisplayResult Edit(ProjectSummaryPart part, BuildPartEditorContext context) =>
            Initialize<ProjectSummaryViewModel>(GetEditorShapeType(context), viewModel => PopulateViewModel(part, viewModel));


        public override async Task<IDisplayResult> UpdateAsync(ProjectSummaryPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new ProjectSummaryViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix);

            part.ProjectName = viewModel.ProjectName;
            part.Day = viewModel.Day;
            part.Month = viewModel.Month;
            part.Year = viewModel.Year;
            part.fileUploadPath = viewModel.fileUploadPath;
            if (viewModel.Day.HasValue && viewModel.Month.HasValue && viewModel.Year.HasValue)
            {
                viewModel.StartDateUtc = $"{viewModel.Day}-{viewModel.Month}-{viewModel.Year}";
            }

            if (string.IsNullOrEmpty(viewModel.StartDateUtc))
            {
                updater.ModelState.AddModelError("ProjectSummaryPart.StartDateUtc", "Enter estimated start date.");
            }
            else
            {
                DateTime startDate;
                if (!DateTime.TryParseExact($"{viewModel.Day}-{viewModel.Month}-{viewModel.Year}", "dd-MM-yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out startDate))
                {
                    updater.ModelState.AddModelError("ProjectSummaryPart.StartDateUtc", "The start date is not valid.");
                }

                if (DateTime.MinValue != startDate && DateTime.UtcNow > startDate)
                {
                    updater.ModelState.AddModelError("ProjectSummaryPart.StartDateUtc", "The start date cannot be in the past.");
                }
            }

            return await EditAsync(part, context);
        }

        private static void PopulateViewModel(ProjectSummaryPart part, ProjectSummaryViewModel viewModel)
        {
            viewModel.ProjectSummaryPart = part;

            viewModel.ProjectName = part.ProjectName;
            viewModel.Day = part.Day;
            viewModel.Month = part.Month;
            viewModel.Year = part.Year;
            viewModel.fileUploadPath = part.fileUploadPath;
            if (part.Day.HasValue && part.Month.HasValue && part.Year.HasValue)
            {
                viewModel.StartDateUtc = $"{part.Day}-{part.Month}-{part.Year}";
            }
        }
    }
}

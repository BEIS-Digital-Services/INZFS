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
    public class ProjectSummaryDriver : BaseDriver<ProjectSummaryPart, ProjectSummaryViewModel>
    {
        public override async Task<IDisplayResult> UpdateAsync(ProjectSummaryPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new ProjectSummaryViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix);
        
            part.ProjectName = viewModel.ProjectName;
            part.Day = viewModel.Day;
            part.Month = viewModel.Month;
            part.Year = viewModel.Year;
            if (viewModel.Day.HasValue && viewModel.Month.HasValue && viewModel.Year.HasValue)
            {
                viewModel.StartDateUtc = $"{viewModel.Day}/{viewModel.Month}/{viewModel.Year}";
            }

            if (string.IsNullOrEmpty(viewModel.StartDateUtc))
            {
                updater.ModelState.AddModelError("ProjectSummaryPart.StartDateUtc", "Enter estimated start date.");
            }
            else
            {
                DateTime startDate;
                if (!DateTime.TryParse($"{viewModel.StartDateUtc}", out startDate))
                {
                    updater.ModelState.AddModelError("ProjectSummaryPart.StartDateUtc", "The start date is not valid.");
                }

                if ( DateTime.MinValue != startDate && DateTime.UtcNow > startDate)
                {
                    updater.ModelState.AddModelError("ProjectSummaryPart.StartDateUtc", "The start date cannot be in the past.");
                }
            }
            
            return await EditAsync(part, context);
        }
        
        protected override void PopulateViewModel(ProjectSummaryPart part, ProjectSummaryViewModel viewModel)
        {
            viewModel.ProjectSummaryPart = part;

            viewModel.ProjectName = part.ProjectName;
            viewModel.Day = part.Day;
            viewModel.Month = part.Month;
            viewModel.Year = part.Year;
            if (part.Day.HasValue && part.Month.HasValue && part.Year.HasValue)
            {
                viewModel.StartDateUtc = $"{part.Day}-{part.Month}-{part.Year}";
            }
        }
    }
}

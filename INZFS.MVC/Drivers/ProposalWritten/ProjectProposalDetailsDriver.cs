using INZFS.MVC.Models.ProposalWritten;
using INZFS.MVC.ViewModels.ProposalWritten;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Drivers.ProposalWritten
{
    public class ProjectProposalDetailsDriver : BaseDriver<ProjectProposalDetailsPart, ProjectProposalDetailsViewModel>
    {
        public override async Task<IDisplayResult> UpdateAsync(ProjectProposalDetailsPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new ProjectProposalDetailsViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix);

            part.InnovationImpactSummary = viewModel.InnovationImpactSummary;
            part.Day = viewModel.Day;
            part.Month = viewModel.Month;
            part.Year = viewModel.Year;
            if (viewModel.Day.HasValue && viewModel.Month.HasValue && viewModel.Year.HasValue)
            {
                viewModel.ProjectEndDateUtc = $"{viewModel.Day}/{viewModel.Month}/{viewModel.Year}";
            }

            if (string.IsNullOrEmpty(viewModel.ProjectEndDateUtc))
            {
                updater.ModelState.AddModelError("ProjectProposalDetailsPart.StartDateUtc", "Enter estimated project end date.");
            }
            else
            {
                DateTime startDate;
                if (!DateTime.TryParse($"{viewModel.ProjectEndDateUtc}", out startDate))
                {
                    updater.ModelState.AddModelError("ProjectProposalDetailsPart.ProjectEndDateUtc", "The project end date is not valid.");
                }

                if (DateTime.MinValue != startDate && DateTime.UtcNow > startDate)
                {
                    updater.ModelState.AddModelError("ProjectProposalDetailsPart.ProjectEndDateUtc", "The project end date cannot be in the past.");
                }
            }

            return await EditAsync(part, context);
        }

        protected override void PopulateViewModel(ProjectProposalDetailsPart part, ProjectProposalDetailsViewModel viewModel)
        {
            viewModel.ProjectProposalDetailsPart = part;

            viewModel.InnovationImpactSummary = part.InnovationImpactSummary;
            viewModel.Day = part.Day;
            viewModel.Month = part.Month;
            viewModel.Year = part.Year;
            if (part.Day.HasValue && part.Month.HasValue && part.Year.HasValue)
            {
                viewModel.ProjectEndDateUtc = $"{part.Day}-{part.Month}-{part.Year}";
            }
        }
    }
}

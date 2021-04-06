using INZFS.MVC.Models.ProposalWritten;
using INZFS.MVC.ViewModels.ProposalWritten;
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
    public class ProjectExperienceDriver : BaseDriver<ProjectExperiencePart, ProjectExperienceViewModel>
    {
        public override async Task<IDisplayResult> UpdateAsync(ProjectExperiencePart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new ProjectExperienceViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix);
            part.ExperienceSummary = viewModel.ExperienceSummary;
            return await EditAsync(part, context);
        }

        protected override void PopulateViewModel(ProjectExperiencePart part, ProjectExperienceViewModel viewModel)
        {
            viewModel.ProjectExperiencePart = part;

            viewModel.ExperienceSummary = part.ExperienceSummary;
        }
    }
}

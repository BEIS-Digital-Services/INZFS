using INZFS.MVC;
using INZFS.MVC.Models;
using INZFS.MVC.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System.Threading.Tasks;

namespace INZFS.MVC.Drivers
{
    public class ProjectDetailsDriver : BaseDriver<ProjectDetailsPart, ProjectDetailsViewModel>
    {
        public override async Task<IDisplayResult> UpdateAsync(ProjectDetailsPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new ProjectDetailsViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix);

            part.Summary = viewModel.Summary;
            part.Timing = viewModel.Timing;


            return await EditAsync(part, context);
        }

        protected override void PopulateViewModel(ProjectDetailsPart part, ProjectDetailsViewModel viewModel)
        {
            viewModel.ProjectDetailsPart = part;

            viewModel.Summary = part.Summary;
            viewModel.Timing = part.Timing;

        }
    }
}

using INZFS.MVC.Models;
using INZFS.MVC.Models.ProposalWritten;
using INZFS.MVC.ViewModels.ProposalWritten;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System.Threading.Tasks;

namespace INZFS.MVC.Drivers.ProposalWritten
{
    public class ApplicationDocumentDriver : BaseDriver<ApplicationDocumentPart, ApplicationDocumentViewModel>
    {
        public override async Task<IDisplayResult> UpdateAsync(ApplicationDocumentPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new ApplicationDocumentViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix);
            return await EditAsync(part, context);
        }

        protected override void PopulateViewModel(ApplicationDocumentPart part, ApplicationDocumentViewModel viewModel)
        {
            viewModel.ApplicationDocumentPart = part;

            viewModel.ProjectPlan = part.ProjectPlan;
            viewModel.ExperienceAndSkills = part.ExperienceAndSkills;
        }
    }
}

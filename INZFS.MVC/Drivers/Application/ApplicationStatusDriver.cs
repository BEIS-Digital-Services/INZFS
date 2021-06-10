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
    public class ApplicationStatusDriver : BaseDriver<ApplicationStatusPart, ApplicationStatusViewModel>
    {
        public override async Task<IDisplayResult> UpdateAsync(ApplicationStatusPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new ApplicationStatusViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix);

            part.Approved = viewModel.Approved;
            part.Rejected = viewModel.Rejected;

            return await EditAsync(part, context);
        }

        protected override void PopulateViewModel(ApplicationStatusPart part, ApplicationStatusViewModel viewModel)
        {
            viewModel.ApplicationStatusPart = part;

            viewModel.Approved = part.Approved;
            viewModel.Rejected = part.Rejected;

        }
    }
}
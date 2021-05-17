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
    public class CompanyDetailsDriver : BaseDriver<CompanyDetailsPart, CompanyDetailsViewModel>
    {
        public override async Task<IDisplayResult> UpdateAsync(CompanyDetailsPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new CompanyDetailsViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix);

            part.CompanyName = viewModel.CompanyName;
            part.CompanyNumber = viewModel.CompanyNumber;


            return await EditAsync(part, context);
        }

        protected override void PopulateViewModel(CompanyDetailsPart part, CompanyDetailsViewModel viewModel)
        {
            viewModel.CompanyDetailsPart = part;

            viewModel.CompanyName = part.CompanyName;
            viewModel.CompanyNumber = part.CompanyNumber;

        }
    }
}
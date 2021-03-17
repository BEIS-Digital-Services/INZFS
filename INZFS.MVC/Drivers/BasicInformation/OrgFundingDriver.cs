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
    public class OrgFundingDriver : ContentPartDisplayDriver<OrgFundingPart>
    {
        public override IDisplayResult Display(OrgFundingPart part, BuildPartDisplayContext context) =>
            Initialize<OrgFundingViewModel>(GetDisplayShapeType(context), viewModel => PopulateViewModel(part, viewModel))
                .Location("Detail", "Content:1")
                .Location("Summary", "Content:1");

        public override IDisplayResult Edit(OrgFundingPart part, BuildPartEditorContext context) =>
            Initialize<OrgFundingViewModel>(GetEditorShapeType(context), viewModel => PopulateViewModel(part, viewModel));


        public override async Task<IDisplayResult> UpdateAsync(OrgFundingPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new OrgFundingViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix);

            part.Funding = viewModel.Funding;

            return await EditAsync(part, context);
        }

        private static void PopulateViewModel(OrgFundingPart part, OrgFundingViewModel viewModel)
        {
            viewModel.OrgFundingPart = part;

            viewModel.Funding = part.Funding;

        }
    }
}

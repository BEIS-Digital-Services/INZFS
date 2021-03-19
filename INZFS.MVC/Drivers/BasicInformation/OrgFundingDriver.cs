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

            part.NoFunding = viewModel.NoFunding;
            part.Funders = viewModel.Funders;
            part.FriendsAndFamily = viewModel.FriendsAndFamily;
            part.PublicSectorGrants = viewModel.PublicSectorGrants;
            part.AngelInvestment = viewModel.AngelInvestment;
            part.VentureCapital = viewModel.VentureCapital;
            part.PrivateEquity = viewModel.PrivateEquity;
            part.StockMarketFlotation = viewModel.StockMarketFlotation;

            return await EditAsync(part, context);
        }

        private static void PopulateViewModel(OrgFundingPart part, OrgFundingViewModel viewModel)
        {
            viewModel.OrgFundingPart = part;

            viewModel.NoFunding = part.NoFunding;
            viewModel.Funders = part.Funders;
            viewModel.FriendsAndFamily = part.FriendsAndFamily;
            viewModel.PublicSectorGrants = part.PublicSectorGrants;
            viewModel.AngelInvestment = part.AngelInvestment;
            viewModel.VentureCapital = part.VentureCapital;
            viewModel.PrivateEquity = part.PrivateEquity;
            viewModel.StockMarketFlotation = part.StockMarketFlotation;
        }
    }
}

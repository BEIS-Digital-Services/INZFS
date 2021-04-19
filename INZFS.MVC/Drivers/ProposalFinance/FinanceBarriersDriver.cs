using INZFS.MVC.Models.ProposalFinance;
using INZFS.MVC.ViewModels.ProposalFinance;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System;
using System.Threading.Tasks;

namespace INZFS.MVC.Drivers.ProposalFinance
{
    public class FinanceBarriersDriver : BaseDriver<FinanceBarriersPart, FinanceBarriersViewModel>
    {
        public override async Task<IDisplayResult> UpdateAsync(FinanceBarriersPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new FinanceBarriersViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix);

            part.Placeholder1 = viewModel.Placeholder1;
            part.Placeholder2 = viewModel.Placeholder2;
            part.Placeholder3 = viewModel.Placeholder3;


            return await EditAsync(part, context);
        }

        protected override void PopulateViewModel(FinanceBarriersPart part, FinanceBarriersViewModel viewModel)
        {
            viewModel.FinanceBarriersPart = part;

            viewModel.Placeholder1 = part.Placeholder1;
            viewModel.Placeholder2 = part.Placeholder2;
            viewModel.Placeholder3 = part.Placeholder3;

        }
    }
}

using INZFS.MVC.Models.ProposalFinance;
using INZFS.MVC.ViewModels.ProposalFinance;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System;
using System.Threading.Tasks;

namespace INZFS.MVC.Drivers.ProposalFinance
{
    public class FinanceRecoverVatDriver : BaseDriver<FinanceRecoverVatPart, FinanceRecoverVatViewModel>
    {
        public override async Task<IDisplayResult> UpdateAsync(FinanceRecoverVatPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new FinanceRecoverVatViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix);

            part.AbleToRecover = viewModel.AbleToRecover;
            
            return await EditAsync(part, context);
        }

        protected override void PopulateViewModel(FinanceRecoverVatPart part, FinanceRecoverVatViewModel viewModel)
        {
            viewModel.FinanceRecoverVatPart = part;

            viewModel.AbleToRecover = part.AbleToRecover;
        }
    }
}

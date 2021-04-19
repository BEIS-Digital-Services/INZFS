using INZFS.MVC.Models.ProposalFinance;
using INZFS.MVC.ViewModels.ProposalFinance;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System;
using System.Threading.Tasks;

namespace INZFS.MVC.Drivers.ProposalFinance
{
    public class FinanceBalanceSheetDriver : BaseDriver<FinanceBalanceSheetPart, FinanceBalanceSheetViewModel>
    {
        public override async Task<IDisplayResult> UpdateAsync(FinanceBalanceSheetPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new FinanceBalanceSheetViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix);

            part.BalanceSheetTotal = viewModel.BalanceSheetTotal;
            part.Day = viewModel.Day;
            part.Month = viewModel.Month;
            part.Year = viewModel.Year;
            if (viewModel.Day.HasValue && viewModel.Month.HasValue && viewModel.Year.HasValue)
            {
                viewModel.BalanceSheetUtc = $"{viewModel.Day}/{viewModel.Month}/{viewModel.Year}";
            }
            
            if (string.IsNullOrEmpty(viewModel.BalanceSheetUtc))
            {
                updater.ModelState.AddModelError("FinanceBalanceSheetPart.BalanceSheetUtc", "Enter balance sheet date.");
            }
            else
            {
                DateTime balanceSheetUtc;
                if (!DateTime.TryParse($"{viewModel.BalanceSheetUtc}", out balanceSheetUtc))
                {
                    updater.ModelState.AddModelError("FinanceBalanceSheetPart.BalanceSheetUtc", "The balance sheet date is not valid.");
                }

                //Is it necessary to validate that a balance sheet date is not in the past?
                //if (DateTime.MinValue != balanceSheetUtc && DateTime.UtcNow > balanceSheetUtc)
                //{
                //    updater.ModelState.AddModelError("FinanceBalanceSheetPart.BalanceSheetUtc", "The balance sheet date cannot be in the past.");
                //}
            }

            return await EditAsync(part, context);
        }

        protected override void PopulateViewModel(FinanceBalanceSheetPart part, FinanceBalanceSheetViewModel viewModel)
        {
            viewModel.FinanceBalanceSheetPart = part;

            viewModel.BalanceSheetTotal = part.BalanceSheetTotal;
            viewModel.Day = part.Day;
            viewModel.Month = part.Month;
            viewModel.Year = part.Year;
            if (part.Day.HasValue && part.Month.HasValue && part.Year.HasValue)
            {
                viewModel.BalanceSheetUtc = $"{part.Day}-{part.Month}-{part.Year}";
            }
        }
    }
}

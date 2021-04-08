using INZFS.MVC.Models.ProposalFinance;
using INZFS.MVC.ViewModels.ProposalFinance;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System;
using System.Threading.Tasks;

namespace INZFS.MVC.Drivers.ProposalFinance
{
    public class FinanceTurnoverDriver : BaseDriver<FinanceTurnoverPart, FinanceTurnoverViewModel>
    {
        public override async Task<IDisplayResult> UpdateAsync(FinanceTurnoverPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new FinanceTurnoverViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix);

            part.TurnoverAmount = viewModel.TurnoverAmount;
            part.Day = viewModel.Day;
            part.Month = viewModel.Month;
            part.Year = viewModel.Year;
            if (viewModel.Day.HasValue && viewModel.Month.HasValue && viewModel.Year.HasValue)
            {
                viewModel.TurnoverUtc = $"{viewModel.Day}/{viewModel.Month}/{viewModel.Year}";
            }

            if (string.IsNullOrEmpty(viewModel.TurnoverUtc))
            {
                updater.ModelState.AddModelError("FinanceTurnoverPart.TurnoverUtc", "Enter turnover date.");
            }
            else
            {
                DateTime turnoverUtc;
                if (!DateTime.TryParse($"{viewModel.TurnoverUtc}", out turnoverUtc))
                {
                    updater.ModelState.AddModelError("FinanceTurnoverPart.TurnoverUtc", "The turnover date is not valid.");
                }

                if (DateTime.MinValue != turnoverUtc && DateTime.UtcNow > turnoverUtc)
                {
                    updater.ModelState.AddModelError("FinanceTurnoverPart.TurnoverUtc", "The turnover date cannot be in the past.");
                }
            }

            return await EditAsync(part, context);
        }

        protected override void PopulateViewModel(FinanceTurnoverPart part, FinanceTurnoverViewModel viewModel)
        {
            viewModel.FinanceTurnoverPart = part;

            viewModel.TurnoverAmount = part.TurnoverAmount;
            viewModel.Day = part.Day;
            viewModel.Month = part.Month;
            viewModel.Year = part.Year;
            if (part.Day.HasValue && part.Month.HasValue && part.Year.HasValue)
            {
                viewModel.TurnoverUtc = $"{part.Day}-{part.Month}-{part.Year}";
            }
        }
    }
}

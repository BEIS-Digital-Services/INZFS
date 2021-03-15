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
    public class ProposalSummaryPartDisplayDriver : ContentPartDisplayDriver<ProposalSummaryPart>
    {
        public override IDisplayResult Display(ProposalSummaryPart part, BuildPartDisplayContext context) =>
            Initialize<ProposalSummaryPartViewModel>(GetDisplayShapeType(context), viewModel => PopulateViewModel(part, viewModel))
                .Location("Detail", "Content:1")
                .Location("Summary", "Content:1");

        public override IDisplayResult Edit(ProposalSummaryPart part, BuildPartEditorContext context) =>
            Initialize<ProposalSummaryPartViewModel>(GetEditorShapeType(context), viewModel => PopulateViewModel(part, viewModel));

        // NEXT STATION: Startup.cs and find the static constructor.

        // So we had an Edit (or EditAsync) method that generates the editor shape. Now it's time to do the content
        // part-specific model binding and validation.
        public override async Task<IDisplayResult> UpdateAsync(ProposalSummaryPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new ProposalSummaryPartViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix);

            part.EstimatedStartDateUtc = viewModel.EstimatedStartDateUtc;
            part.ProjectName = viewModel.ProjectName;
            part.Summary = viewModel.Summary;
            part.ProjectDuration = viewModel.ProjectDuration;

            return await EditAsync(part, context);
        }

        private static void PopulateViewModel(ProposalSummaryPart part, ProposalSummaryPartViewModel viewModel)
        {
            viewModel.ProposalSummaryPart = part;

            viewModel.EstimatedStartDateUtc = part.EstimatedStartDateUtc;
            viewModel.ProjectName = part.ProjectName;
            viewModel.Summary = part.Summary;
            viewModel.ProjectDuration = part.ProjectDuration;
        }
    }
}

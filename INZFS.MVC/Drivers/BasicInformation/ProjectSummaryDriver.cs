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
    public class ProjectSummaryDriver : ContentPartDisplayDriver<ProjectSummaryPart>
    {
        public override IDisplayResult Display(ProjectSummaryPart part, BuildPartDisplayContext context) =>
            Initialize<ProjectSummaryViewModel>(GetDisplayShapeType(context), viewModel => PopulateViewModel(part, viewModel))
                .Location("Detail", "Content:1")
                .Location("Summary", "Content:1");

        public override IDisplayResult Edit(ProjectSummaryPart part, BuildPartEditorContext context) =>
            Initialize<ProjectSummaryViewModel>(GetEditorShapeType(context), viewModel => PopulateViewModel(part, viewModel));

      
        public override async Task<IDisplayResult> UpdateAsync(ProjectSummaryPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new ProjectSummaryViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix);

        
            part.ProjectName = viewModel.ProjectName;
            part.Day = viewModel.Day;
            part.Month = viewModel.Month;
            part.Year = viewModel.Year;


            return await EditAsync(part, context);
        }

        private static void PopulateViewModel(ProjectSummaryPart part, ProjectSummaryViewModel viewModel)
        {
            viewModel.ProjectSummaryPart = part;

            viewModel.ProjectName = part.ProjectName;
            viewModel.Day = part.Day;
            viewModel.Month = part.Month;
            viewModel.Year = part.Year;
 
        }
    }
}

using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Drivers
{
    public abstract class BaseDriver<TPart, TModel> : ContentPartDisplayDriver<TPart> 
                        where TModel : class
                        where TPart : ContentPart, new()
    {
        public override IDisplayResult Display(TPart part, BuildPartDisplayContext context) =>
           Initialize<TModel>(GetDisplayShapeType(context), viewModel => PopulateViewModel(part, viewModel))
               .Location("Detail", "Content:1")
               .Location("Summary", "Content:1");

        public override IDisplayResult Edit(TPart part, BuildPartEditorContext context) =>
            Initialize<TModel>(GetEditorShapeType(context), viewModel => PopulateViewModel(part, viewModel));

        protected abstract void PopulateViewModel(TPart part, TModel viewModel);
    }
}

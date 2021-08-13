using INZFS.MVC.Models.DynamicForm;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.ModelProviders
{
    
    public class BaseModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType != typeof(BaseModel))
            {
                return null;
            }

            var subclasses = new[] { typeof(TextInputModel), typeof(TextAreaModel), 
                typeof(DateModel), typeof(MultiSelectInputModel),
                typeof(YesornoInputModel), typeof(FileUploadModel), typeof(CurrencyInputModel), 
                typeof(RadioSingleSelectModel), typeof(AddressInputModel)
                };

            var binders = new Dictionary<Type, (ModelMetadata, IModelBinder)>();
            foreach (var type in subclasses)
            {
                var modelMetadata = context.MetadataProvider.GetMetadataForType(type);
                binders[type] = (modelMetadata, context.CreateBinder(modelMetadata));
            }

            return new DeviceModelBinder(binders);
        }
    }

    public class DeviceModelBinder : IModelBinder
    {
        private Dictionary<Type, (ModelMetadata, IModelBinder)> binders;

        public DeviceModelBinder(Dictionary<Type, (ModelMetadata, IModelBinder)> binders)
        {
            this.binders = binders;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var modelKindName = ModelNames.CreatePropertyModelName(bindingContext.ModelName, nameof(BaseModel.Kind));
            var modelTypeValue = bindingContext.ValueProvider.GetValue(modelKindName).FirstValue;

            IModelBinder modelBinder;
            ModelMetadata modelMetadata;
            if (modelTypeValue == nameof(TextInputModel))
            {
                (modelMetadata, modelBinder) = binders[typeof(TextInputModel)];
            }
            else if (modelTypeValue == nameof(TextAreaModel))
            {
                (modelMetadata, modelBinder) = binders[typeof(TextAreaModel)];
            }
            else if (modelTypeValue == nameof(DateModel))
            {
                (modelMetadata, modelBinder) = binders[typeof(DateModel)];
            }
            else if (modelTypeValue == nameof(YesornoInputModel))
            {
                (modelMetadata, modelBinder) = binders[typeof(YesornoInputModel)];
            }
            else if (modelTypeValue == nameof(MultiSelectInputModel))
            {
                (modelMetadata, modelBinder) = binders[typeof(MultiSelectInputModel)];
            }
            else if (modelTypeValue == nameof(FileUploadModel))
            {
                (modelMetadata, modelBinder) = binders[typeof(FileUploadModel)];
            }
            else if (modelTypeValue == nameof(CurrencyInputModel))
            {
                (modelMetadata, modelBinder) = binders[typeof(CurrencyInputModel)];
            }
            else if (modelTypeValue == nameof(RadioSingleSelectModel))
            {
                (modelMetadata, modelBinder) = binders[typeof(RadioSingleSelectModel)];
            }
            else if (modelTypeValue == nameof(AddressInputModel))
            {
                (modelMetadata, modelBinder) = binders[typeof(AddressInputModel)];
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            var newBindingContext = DefaultModelBindingContext.CreateBindingContext(
                bindingContext.ActionContext,
                bindingContext.ValueProvider,
                modelMetadata,
                bindingInfo: null,
                bindingContext.ModelName);

            await modelBinder.BindModelAsync(newBindingContext);
            bindingContext.Result = newBindingContext.Result;

            if (newBindingContext.Result.IsModelSet)
            {
                // Setting the ValidationState ensures properties on derived types are correctly 
                bindingContext.ValidationState[newBindingContext.Result.Model] = new ValidationStateEntry
                {
                    Metadata = modelMetadata,
                };
            }
        }
    }
}

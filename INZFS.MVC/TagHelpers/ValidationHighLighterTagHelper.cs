using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Threading.Tasks;

namespace INZFS.MVC.TagHelpers
{
    //[HtmlTargetElement("*", Attributes = ValidationForAttributeName + "," + ErrorClassAttributeName)]
    [HtmlTargetElement("*", Attributes = ValidationForAttributeName)]
    public class ValidationHighLighterTagHelper : TagHelper
    {
        private const string ValidationForAttributeName = "asp-validation-error-class-for";
        private const string HasValidationErrorClassName = "govuk-form-group--error";
        private const string ErrorClassAttributeName = "asp-error-class";
        private readonly IHtmlHelper _htmlHelper;

        public ValidationHighLighterTagHelper(IHtmlHelper htmlHelper)
        {
            _htmlHelper = htmlHelper;
        }

        /// <inheritdoc />
        public override int Order
        {
            get
            {
                return -1000;
            }
        }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// Name to be validated on the current model.
        /// </summary>
        [HtmlAttributeName(ValidationForAttributeName)]
        public ModelExpression For { get; set; }

        //[HtmlAttributeName(ErrorClassAttributeName)]
        //public string ErrorClass{ get; set; }

        /// <inheritdoc />
        /// <remarks>Does nothing if <see cref="For"/> is <c>null</c>.</remarks>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            if (For != null)
            {
                //contextualize IHtmlHelper
                var viewContextAware = _htmlHelper as IViewContextAware;
                viewContextAware?.Contextualize(ViewContext);

                var fullName = For.Metadata.ContainerType.Name.Replace("ViewModel", "Part") + "."  + _htmlHelper.GenerateIdFromName(For.Name);

                if (ViewContext.ViewData.ModelState.TryGetValue(fullName, out var entry) && entry.Errors.Count > 0)
                {
                    TagHelperAttribute classAttribute;

                    //var errorClassName = string.IsNullOrEmpty(ErrorClass) ? HasValidationErrorClassName : ErrorClass;
                    var errorClassName = HasValidationErrorClassName;

                    if (output.Attributes.TryGetAttribute("class", out classAttribute))
                    {
                        output.Attributes.SetAttribute("class", classAttribute.Value + " " + errorClassName);
                    }
                    else
                    {
                        output.Attributes.Add("class", errorClassName);
                    }
                }

                // We check for whitespace to detect scenarios such as:
                // <span validation-for="Name">
                // </span>
                if (!output.IsContentModified)
                {
                    var childContent = await output.GetChildContentAsync();
                    output.Content.SetHtmlContent(childContent);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace INZFS.Theme.TagHelpers
{
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

        [HtmlAttributeName(ValidationForAttributeName)]
        public ModelExpression For { get; set; }

       
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
                var viewContextAware = _htmlHelper as IViewContextAware;
                viewContextAware?.Contextualize(ViewContext);

                var fullName = _htmlHelper.GenerateIdFromName(For.Name);
             
                if (ViewContext.ViewData.ModelState.TryGetValue(fullName, out var entry) && entry.Errors.Count > 0)
                {
                    TagHelperAttribute classAttribute;

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

                if (!output.IsContentModified)
                {
                    var childContent = await output.GetChildContentAsync();
                    output.Content.SetHtmlContent(childContent);
                }
            }
        }
    }
}


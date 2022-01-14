using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace INZFS.Theme.TagHelpers
{

    [HtmlTargetElement("*", Attributes = ValidationForAttributeName)]
    public class CssClassReplacerTagHelper : TagHelper
    {
        private const string ValidationForAttributeName = "asp-validation-govuk-for";
        private const string HasValidationErrorClassName = "govuk-input--error";
        private readonly IHtmlHelper _htmlHelper;

        public CssClassReplacerTagHelper(IHtmlHelper htmlHelper) => this._htmlHelper = htmlHelper;

        /// <inheritdoc />
        public override int Order => -1000;

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <summary>Name to be validated on the current model.</summary>
        [HtmlAttributeName(ValidationForAttributeName)]
        public ModelExpression For { get; set; }

        /// <inheritdoc />
        /// <remarks>Does nothing if <see cref="P:OrchardCore.DisplayManagement.TagHelpers.ValidationMessageTagHelper.For" /> is <c>null</c>.</remarks>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            if (this.For == null)
                return;
            if (this._htmlHelper is IViewContextAware htmlHelper)
                htmlHelper.Contextualize(this.ViewContext);

            ModelStateEntry modelStateEntry;
            if (this.ViewContext.ViewData.ModelState.TryGetValue(this._htmlHelper.GenerateIdFromName(this.For.Name), out modelStateEntry) && modelStateEntry.Errors.Count > 0)
            {
                TagHelperAttribute attribute;
                if (output.Attributes.TryGetAttribute("class", out attribute))
                {
                    var newAttribute = new TagHelperAttribute(
                        attribute.Name,
                        new ClassAttributeHtmlContent(attribute.Value, HasValidationErrorClassName),
                        attribute.ValueStyle);

                    output.Attributes.SetAttribute(newAttribute);

                }
                    
                else
                    output.Attributes.Add("class", (object)HasValidationErrorClassName);
            }
            if (output.IsContentModified)
                return;
            output.Content.SetHtmlContent((IHtmlContent)await output.GetChildContentAsync());
        }

        private class ClassAttributeHtmlContent : IHtmlContent
        {
            private readonly object _left;
            private readonly string _right;

            public ClassAttributeHtmlContent(object left, string right)
            {
                _left = left;
                _right = right;
            }

            public void WriteTo(TextWriter writer, HtmlEncoder encoder)
            {
                if (writer == null)
                {
                    throw new ArgumentNullException(nameof(writer));
                }

                if (encoder == null)
                {
                    throw new ArgumentNullException(nameof(encoder));
                }

                // Write out "{left} {right}" in the common nothing-empty case.
                var wroteLeft = false;
                if (_left != null)
                {
                    if (_left is IHtmlContent htmlContent)
                    {
                        // Ignore case where htmlContent is HtmlString.Empty. At worst, will add a leading space to the
                        // generated attribute value.
                        htmlContent.WriteTo(writer, encoder);
                        wroteLeft = true;
                    }
                    else
                    {
                        var stringValue = _left.ToString();
                        if (!string.IsNullOrEmpty(stringValue))
                        {
                            encoder.Encode(writer, stringValue);
                            wroteLeft = true;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(_right))
                {
                    if (wroteLeft)
                    {
                        writer.Write(' ');
                    }

                    encoder.Encode(writer, _right);
                }
            }
        }

    }
}


//    [HtmlTargetElement("*", Attributes = AttributeName)]
//    public class CssClassReplacerTagHelper : TagHelper
//    {
//        private const string AttributeName = "asp-replace-class";

//        [HtmlAttributeName(AttributeName)]
//        public string ClassName { get; set; }

//        public override void Process(TagHelperContext context, TagHelperOutput output)
//        {
//            if (output.Attributes.TryGetAttribute("class", out var classAttribute) && 
//                (classAttribute?.Value?.ToString()?.EndsWith("+ClassAttributeHtmlContent") ?? false))
//            {
//                var newAttribute = new TagHelperAttribute(
//                    classAttribute.Name,
//                    new ClassAttributeHtmlContent(classAttribute.Value, ClassName),
//                    classAttribute.ValueStyle);

//                output.Attributes.SetAttribute(newAttribute);
//            }

//        }


       

    
//}

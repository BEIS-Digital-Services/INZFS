using Galebra.Security.Headers.Csp.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Linq;

namespace Galebra.Security.Headers.Csp
{ 

/// <summary>
/// Displays the <see cref="CspPolicyGroup"/> name that is used in the request
/// </summary>
public sealed class DisplayCspGroupTagHelper : TagHelper
{
    private readonly ICspOptions _cspOptions;
    private readonly KeyValuePair<string, CspPolicyGroup> _defaultPolicyGroup;

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;//Should be public or it is null

    public DisplayCspGroupTagHelper(ICspOptions cspOptions)
    {
        _cspOptions = cspOptions;
        _defaultPolicyGroup = _cspOptions.PolicyGroups.First(x => x.Value.IsDefault);
    }

    /// <summary>
    /// Gets or sets the <see cref="Rendering.ViewContext"/> for the current request.
    /// </summary>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
          var message = TagHelperBuilderService
            .ApplyOutput(_cspOptions, _defaultPolicyGroup, ViewContext, output, (_, _) => { });
        output.TagName = string.Empty;
        output.Content.SetContent(message);
    }
}

}
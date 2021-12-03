using Galebra.Security.Headers.Csp.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Linq;

namespace Galebra.Security.Headers.Csp
{ 

/// <summary>
/// Adds a content security policy nonce to this tag
/// </summary>
[HtmlTargetElement("script", Attributes = "nonce-add")]
[HtmlTargetElement("style", Attributes = "nonce-add")]
[HtmlTargetElement("link", Attributes = "nonce-add",
TagStructure = TagStructure.WithoutEndTag)]
public sealed class CspNonceTagHelper : TagHelper
{
    private readonly ICspOptions _cspOptions;
    private readonly ICspNonce _cspNonce;
    private readonly KeyValuePair<string, CspPolicyGroup> _defaultPolicyGroup;

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;//Should be public or it is null

    /// <summary>
    /// Boolean to add a content security policy nonce to the tag
    /// </summary>
    [HtmlAttributeName("nonce-add")]
    public bool AddNonce { get; set; }


    public CspNonceTagHelper(ICspOptions cspOptions, ICspNonce cspNonce)
    {
        _cspOptions = cspOptions;
        _defaultPolicyGroup = _cspOptions.PolicyGroups.First(x => x.Value.IsDefault);
        _cspNonce = cspNonce;
    }

    /// <summary>
    /// Gets or sets the <see cref="Rendering.ViewContext"/> for the current request.
    /// </summary>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (!AddNonce)
        {
            return;
        }

        ////For disable attribute or filter
        //var disableRequired = ViewContext.HttpContext.Items
        //     .TryGetValue(CspConstants.DisableCspResultFilterAttributeKey, out var disableObj);
        //if (disableObj is bool enforceMode)
        //{
        //    if (enforceMode)
        //    {
        //        return;
        //    }
        //    //enforceMode is false here, so let the pipeline continue
        //}
        //else
        //{
        //    //enforceMode is null, which should never happen since we always set it in attribute or filter
        //}

        ////For enable attribute or filter
        //if (ViewContext.HttpContext.Items
        //    .TryGetValue(CspConstants.EnableCspResultFilterAttributeKey, out var obj))
        //{
        //    if (obj is string cspPolicyGroupName)
        //    {
        //        SetTagHelperOutput(cspPolicyGroupName, output);
        //    }
        //    else
        //    {
        //        // we have a null cspPolicyGroupName whereas the EnableCspResultFilterAttributeKey is
        //        // here, so this is an attribute or filter with a null parameter (default), e.g. [EnableCsp]
        //        SetTagHelperOutput(_defaultPolicyGroup.Key, output);
        //    }
        //}
        //else
        //{
        //    //EnableCspResultFilterAttributeKey not found, so use default, unless all policies are disabled
        //    if (!_cspOptions.IsDisabled)
        //    {
        //        if (disableRequired)
        //        {
        //            // There is a disable attribute or filter, no matter the enforce mode,
        //            // so do not set
        //        }
        //        else
        //        {
        //            SetTagHelperOutput(_defaultPolicyGroup.Key, output);
        //        }
        //    }
        //    else // All policies are disabled
        //    {
        //    }
        //}

        TagHelperBuilderService.ApplyOutput(_cspOptions,_defaultPolicyGroup, ViewContext, output, SetTagHelperOutput);
    }

    private void SetTagHelperOutput(string name, TagHelperOutput output)
    {
        if (_cspOptions.PolicyGroups[name].RequiresNonceForAtLeastOne)
        {
            output.Attributes.SetAttribute("nonce", _cspNonce.Nonces[name].Nonce);
        }
    }
}

}

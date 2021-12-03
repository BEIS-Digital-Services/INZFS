using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;

namespace Galebra.Security.Headers.Csp.Infrastructure
{
    internal static class TagHelperBuilderService
    {
        /// <summary>
        /// The flow logic herein should match the one in the <see cref="CspMiddleware"/> object
        /// </summary>
        internal static string ApplyOutput(ICspOptions cspOptions, KeyValuePair<string, CspPolicyGroup> defaultPolicyGroup,
            ViewContext viewContext, TagHelperOutput output, Action<string, TagHelperOutput> setupAction)
        {
            //For disable attribute or filter
            var disableRequired = viewContext.HttpContext.Items
                 .TryGetValue(CspConstants.DisableCspResultFilterAttributeKey, out var disableObj);
            if (disableObj is bool enforceMode)
            {
                if (enforceMode)
                {
                    return CspConstants.CspDisabled;
                }
                //enforceMode is false here, so let the pipeline continue
            }
            else
            {
                //enforceMode is null, which should never happen since we always set it in attribute or filter
                //so let the pipeline continue
            }

            //For enable attribute or filter
            if (viewContext.HttpContext.Items
                .TryGetValue(CspConstants.EnableCspResultFilterAttributeKey, out var obj))
            {
                if (obj is string cspPolicyGroupName)
                {
                    setupAction(cspPolicyGroupName, output);
                    return cspPolicyGroupName;
                }
                else
                {
                    // we have a null cspPolicyGroupName whereas the EnableCspResultFilterAttributeKey is
                    // here, so this is an attribute or filter with a null parameter (default), e.g. [EnableCsp]
                    setupAction(defaultPolicyGroup.Key, output);
                    return defaultPolicyGroup.Key;
                }
            }
            else
            {
                //EnableCspResultFilterAttributeKey not found, so use default, unless all policies are disabled
                if (!cspOptions.IsDisabled)
                {
                    if (disableRequired)
                    {
                        // There is a disable attribute or filter, no matter the enforce mode,
                        // so do not set
                        return CspConstants.CspDisabled;
                    }
                    else
                    {
                        setupAction(defaultPolicyGroup.Key, output);
                        return defaultPolicyGroup.Key;

                    }
                }
                else // All policies are disabled
                {
                    return CspConstants.CspDisabledGlobal;
                }
            }
        }
    }
}

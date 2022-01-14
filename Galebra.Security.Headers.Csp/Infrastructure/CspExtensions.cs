using Galebra.Security.Headers.Csp.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Galebra.Security.Headers.Csp.Infrastructure
{ 

internal static class CspExtensions
{
    /// <summary>
    /// Checks if the <see cref="CspOptions.PolicyGroups"/>
    /// configured by the user are well formed and 
    /// builds the <paramref name="cspOptions"/>
    /// </summary>
    /// <param name="cspOptions"></param>
    internal static void Build(this CspOptions cspOptions)
    {
        if (cspOptions is null)
        {
            throw new ArgumentNullException(nameof(cspOptions));
        }

        //Will throw if the config Key is not valid in appsettings, or if PolicyGroups is not
        //named properly
        if (cspOptions.PolicyGroups.Count == 0)
        {
            throw new Exception($"Provide at least one policy group. " +
                $"Review the CSP configuration in appsettings.json file or Startup class. " +
                $"{nameof(ICspOptions.PolicyGroups)} is empty.");
        }

        //---We have at least one policy---
        /// Check if only one policy group has a default. We could have used Single
        /// but we use this method to provide a friendly error message
        var hasOneDefault = cspOptions.HasOneDefault();
        if (!hasOneDefault.Success)
        {
            throw new ArgumentOutOfRangeException(nameof(cspOptions),
                $"{hasOneDefault.Count} policy groups are set as default. " +
                $"Provide exactly one default policy group. " +
                   $"{nameof(cspOptions.HasOneDefault)} returned {hasOneDefault}");
        }

        foreach (var kvpPolicyGroup in cspOptions.PolicyGroups)
        {
            kvpPolicyGroup.Sanitize();
        }
    }

    /// <summary>
    /// <inheritdoc cref="Sanitize(CspPolicyGroup)"/>
    /// </summary>
    /// <param name="kvpPolicyGroup"></param>
    private static void Sanitize(this KeyValuePair<string, CspPolicyGroup> kvpPolicyGroup)
    {
        //Sanitize the Key
        //This is to allow for a flexible debugging.
        //For example if the keys are sent as header names, the
        //http pipeline will throw a proptocol error if a key is empty or whitespace
        if (string.IsNullOrWhiteSpace(kvpPolicyGroup.Key))
        {
            throw new ArgumentOutOfRangeException(kvpPolicyGroup.Key,
                $"Policy group Key {kvpPolicyGroup.Key} in {nameof(CspOptions.PolicyGroups)} " +
                $"cannot be null, empty or whitespace");
        }

        //Sanitize the value
        //Throw if semicolons found at start of string.
        //Trim and remove items in Nonceable if they are empty
        kvpPolicyGroup.Value.Sanitize();

        //Now all Nonceable are either empty (no items) or have non-empty items
        //Check for simulataneous emptiness
        if (kvpPolicyGroup.Value.IsEmpty())
        {
            throw new ArgumentOutOfRangeException(kvpPolicyGroup.Key, $"{kvpPolicyGroup.Key} is empty. " +
                $"Provide at least one csp policy, or one csp report only policy, or both. " +
                $"Or remove this group all together");

        }
        else //This policy group is not empty
        {
            //At this stage, Fixed and Nonceable are not empty at the same time
            //Data here are considered valid
        }
    }

    /// <summary>
    /// <inheritdoc cref="Sanitize(CspPolicy)"/>
    /// </summary>
    /// <returns></returns>
    internal static CspPolicyGroup Sanitize(this CspPolicyGroup cspPolicyGroup)
    {
        if (cspPolicyGroup is null)
        {
            throw new ArgumentNullException(nameof(cspPolicyGroup));
        }

        cspPolicyGroup.Csp.Sanitize();
        cspPolicyGroup.CspReportOnly.Sanitize();

        //Add a nonce flag based on whether the user configured any
        cspPolicyGroup.Csp.FlagForNonce();
        cspPolicyGroup.CspReportOnly.FlagForNonce();
        cspPolicyGroup.FlagForAtLeastOneNonce();
        cspPolicyGroup.PrepareHeaderValue();

        return cspPolicyGroup;
    }
    /// <summary>
    /// Performs a check and reformatting of user entries
    /// </summary>
    /// <returns></returns>
    internal static CspPolicy Sanitize(this CspPolicy cspPolicy)
    {
        if (cspPolicy is null)
        {
            throw new ArgumentNullException(nameof(cspPolicy));
        }

        cspPolicy.Fixed = cspPolicy.Fixed.TrimEmptyfy();
        Helpers.CheckData(cspPolicy.Fixed, nameof(cspPolicy.Fixed));


        cspPolicy.Nonceable = cspPolicy.Nonceable.ToList().RemoveAllTrimEmptyfy();
        Helpers.CheckData(cspPolicy.Nonceable, nameof(cspPolicy.Nonceable));
        //Now we have non-null and trimmed directive values
        return cspPolicy;
    }

    internal static void PrepareHeaderValue(this CspPolicyGroup cspPolicyGroup)
    {
        if (cspPolicyGroup is null)
        {
            throw new ArgumentNullException(nameof(cspPolicyGroup));
        }

        cspPolicyGroup.Csp.SetFixedHeaderValue();
        cspPolicyGroup.Csp.SetNonceableHeaderValuePrefix();

        cspPolicyGroup.CspReportOnly.SetFixedHeaderValue();
        cspPolicyGroup.CspReportOnly.SetNonceableHeaderValuePrefix();
    }
    private static void SetFixedHeaderValue(this CspPolicy cspPolicy)
    {
        if (cspPolicy is null)
        {
            throw new ArgumentNullException(nameof(cspPolicy));
        }

        if (cspPolicy.RequiresNonce)
        {
            if (string.IsNullOrWhiteSpace(cspPolicy.Fixed))
            {
                //Avoid starting with semicolon
                cspPolicy.Fixed = string.Empty;
            }
            else
            {
                cspPolicy.Fixed = string.Concat(cspPolicy.Fixed, ';');
            }
        }
        else
        {
            //No nonce required, so just put Fixed without ending with semicolon
            cspPolicy.Fixed = cspPolicy.Fixed;
        }
    }
    /// <summary>
    /// Prepares the policies in <see cref="CspPolicy.Nonceable"/>
    /// to accept a <see cref="INonceGenerator.NonceHeader"/>, i.e.
    /// a string of the form 'nonce-12345'
    /// </summary>
    /// <param name="cspPolicy"></param>
    private static void SetNonceableHeaderValuePrefix(this CspPolicy cspPolicy)
    {
        if (cspPolicy is null)
        {
            throw new ArgumentNullException(nameof(cspPolicy));
        }
        if (cspPolicy.RequiresNonce)//Don't add if nonce is not required, otherwise we fill with a whitespace
        {
            cspPolicy.Nonceable = cspPolicy.Nonceable.ToList().ConvertAll(s => string.Concat(s, " "));
        }
    }
}

}
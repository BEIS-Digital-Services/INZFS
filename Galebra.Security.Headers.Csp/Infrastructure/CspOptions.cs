using System;
using System.Collections.Generic;
using System.Linq;

namespace Galebra.Security.Headers.Csp.Infrastructure
{ 

/// <summary>
/// <inheritdoc cref="ICspOptions"/>
/// </summary>
internal sealed class CspOptions : ICspOptions, ICspOptionsBuilder
{
    public bool IsDisabled { get; set; }
    public IDictionary<string, CspPolicyGroup> PolicyGroups { get; }
        = new Dictionary<string, CspPolicyGroup>();
    public void Add(string policyGroupName, Action<ICspPolicyGroup> policyAction)
    {
        if (policyGroupName is null)
        {
            throw new ArgumentNullException(nameof(policyGroupName));
        }

        if (policyAction is null)
        {
            throw new ArgumentNullException(nameof(policyAction));
        }
        var cspPolicyGroup = new CspPolicyGroup();
        policyAction(cspPolicyGroup);

        PolicyGroups.Add(policyGroupName, cspPolicyGroup);
    }

    /// <summary>
    /// Returns true if this <see cref="PolicyGroups"/> has exactly one <see cref="CspPolicyGroup"/>
    /// that is set as default through <see cref="CspPolicyGroup.IsDefault"/>
    /// </summary>
    /// <returns></returns>
    internal (bool Success, int Count) HasOneDefault()
    {
        var count = PolicyGroups.Count(pg => pg.Value.IsDefault == true);
        return (count == 1, count);
    }
}

}
using Galebra.Security.Headers.Csp.Infrastructure;
using System.Collections.Generic;

namespace Galebra.Security.Headers.Csp
{ 

/// <summary>
/// Configurations options for content security policy
/// </summary>
public interface ICspOptions
{
    /// <summary>
    /// Represents a dictionary of policy groups
    /// Keys cannot be null, empty or filled with whitespace only.
    /// </summary>
    IDictionary<string, CspPolicyGroup> PolicyGroups { get; }

    /// <summary>
    /// If the result is true, content security policy is not applied globally, until overwritten
    /// by <see cref="EnableCspAttribute"/> or <see cref="EnableCspPageFilter"/>.
    /// Defaults to false.
    /// </summary>
    bool IsDisabled { get; }
}

}
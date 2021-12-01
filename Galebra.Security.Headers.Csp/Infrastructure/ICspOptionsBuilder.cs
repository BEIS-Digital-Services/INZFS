using System;

namespace Galebra.Security.Headers.Csp.Infrastructure
{ 

public interface ICspOptionsBuilder
{
    /// <summary>
    /// Adds a content security policy group
    /// </summary>
    /// <param name="policyGroupName">The name of the policy group</param>
    /// <param name="policyAction">The <see cref="ICspPolicyGroup"/> to configure</param>
    void Add(string policyGroupName, Action<ICspPolicyGroup> policyAction);

    /// <summary>
    /// When true, disables all content security policies globally. Defaults to false (recommended).
    /// Set to true in order to apply Content Security Policy only on a subset of pages.
    /// To be used in conjunction with <see cref="EnableCspAttribute"/>
    /// or <see cref="EnableCspPageFilter"/> to apply content security policies on specific pages,
    /// folders, actions or controllers.
    /// </summary>
    bool IsDisabled { set; }

}

}
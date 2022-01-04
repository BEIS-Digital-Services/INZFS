namespace Galebra.Security.Headers.Csp
{ 

/// <summary>
/// Enables Content Security Policy for <see cref="PolicyGroupName"/>.<br/>
/// Has no effect if <see cref="IDisableCspFilterAttribute"/>
/// is used with <see cref="IDisableCspFilterAttribute.EnforceMode"/>
/// set to true.
/// </summary>
internal interface IEnableCspFilterAttribute
{
    /// <summary>
    /// The name of the policy group
    /// </summary>
    string? PolicyGroupName { get; init; }
}

}
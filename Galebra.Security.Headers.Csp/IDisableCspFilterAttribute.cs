namespace Galebra.Security.Headers.Csp
{ 

/// <summary>
/// Disables Content Security Policy.<br/>
/// When <see cref="EnforceMode"/> is set to true, disabling CSP takes precedence over
/// <see cref="IEnableCspFilterAttribute"/>.<br/>
/// When set to false, disables CSP only in the absence of
/// <see cref="IEnableCspFilterAttribute"/>.
/// </summary>
internal interface IDisableCspFilterAttribute
{
    /// <summary>
    /// The enforce mode for disabling CSP
    /// </summary>
    bool EnforceMode { get; init; }
}

}
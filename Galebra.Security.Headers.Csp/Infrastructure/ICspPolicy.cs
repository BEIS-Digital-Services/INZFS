using System.Collections.Generic;

namespace Galebra.Security.Headers.Csp.Infrastructure
{ 

/// <summary>
/// Represents a content security policy
/// </summary>
public interface ICspPolicy
{
    /// <summary>
    /// Directive set that does not change upon requests on the url. Hence Fixed. Use this alone if no nonces are required.<br/>
    /// Example: "default-src 'self'; media-src 'none'; style-src https://api.somestyle.com sha256-xieUrkQi03xxxxxxxx"<br/>
    /// Do not add a semicolon at the end of the string
    /// </summary>
    string Fixed { get; set; }

    /// <summary>
    /// List of directives that require a nonce.<br/>
    /// Example: "script-src 'self'"<br/>
    /// Do not add a semicolon at the end of the string
    /// </summary>
    IList<string> Nonceable { get; }
}

}
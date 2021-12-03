using System.Collections.Generic;

namespace Galebra.Security.Headers.Csp.Infrastructure
{ 

/// <summary>
/// <inheritdoc cref="ICspPolicy"/>
/// </summary>
public sealed class CspPolicy : ICspPolicy
{
    public string Fixed { get; set; } = string.Empty;

    public IList<string> Nonceable { get; internal set; } = new List<string>();

    /// <summary>
    /// This flag should be used after <see cref="Sanitize"/>,
    /// otherwise it will flag Nonceables with empty entries
    /// </summary>
    /// <returns></returns>
    internal bool FlagForNonce() => _requiresNonce = Nonceable.Count != 0;

    private bool _requiresNonce;

    /// <summary>
    /// If true this will require a nonce in the middleware and taghelper.<br/>
    /// Rather than being determined on every request, the value is determined once prior to being injected
    /// in the middlware singleton instance.
    /// </summary>
    internal bool RequiresNonce => _requiresNonce;

    /// <summary>
    /// Returns true if the <see cref="CspPolicy"/> has no items in <see cref="Nonceable"/><br/>
    /// and if <see cref="Fixed"/> is null, empty or has only whitespace<br/>
    /// This method should be used after <see cref="Sanitize"/>,
    /// otherwise it will consider Nonceables with empty entries as non empty
    /// </summary>
    /// <returns></returns>
    internal bool IsEmpty()
    {
        //Use Count over Any() as it is 10x faster
        return string.IsNullOrWhiteSpace(Fixed) && Nonceable.Count == 0;
    }
}

}
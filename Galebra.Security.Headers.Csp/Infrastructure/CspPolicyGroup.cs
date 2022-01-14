namespace Galebra.Security.Headers.Csp.Infrastructure
{ 

/// <summary>
/// <inheritdoc cref="ICspPolicyGroup"/>
/// </summary>
public sealed class CspPolicyGroup : ICspPolicyGroup
{
    public CspPolicy Csp { get; } = new CspPolicy();
    public CspPolicy CspReportOnly { get; } = new CspPolicy();
    public bool IsDefault { get; set; } = true;

    public int NumberOfNonceBytes { get; set; } = 16;


    /// <summary>
    /// This flag should be used after <see cref="CspPolicy.Sanitize"/>,
    /// otherwise it will flag Nonceables with whitespace entries
    /// </summary>
    /// <returns></returns>
    internal bool FlagForAtLeastOneNonce() =>
        _requiresNonceForAtLeastOne = Csp.Nonceable.Count != 0 || CspReportOnly.Nonceable.Count != 0;

    private bool _requiresNonceForAtLeastOne;

    /// <summary>
    /// Returns true if <see cref="CspPolicy.Csp"/> or <see cref="CspPolicy.CspReportOnly"/>
    /// requires a nonce.
    /// This is useful for Taghelpers
    /// </summary>
    internal bool RequiresNonceForAtLeastOne => _requiresNonceForAtLeastOne;
    /// <summary>
    /// Returns true if all policies in this <see cref="CspPolicyGroup"/>
    /// have no items in <see cref="CspPolicy.Nonceable"/>
    /// and if <see cref="CspPolicy.Fixed"/> is a null, empty or has only whitespace<br/>
    /// This method should be used after <see cref="CspExtensions.Sanitize(CspPolicyGroup)"/>,
    /// otherwise it will consider Nonceables with empty entries as non empty
    /// </summary>
    /// <returns></returns>
    internal bool IsEmpty()
    {
        return Csp.IsEmpty() && CspReportOnly.IsEmpty();
    }
}

}
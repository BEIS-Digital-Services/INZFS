namespace Galebra.Security.Headers.Csp.Infrastructure
{ 

/// <summary>
/// Represents a content security policy group
/// Each policy group is composed of one
/// Enforceable and/or one Report-Only content security policies
/// </summary>
public interface ICspPolicyGroup
{
    /// <summary>
    /// The content security policy to enforce.
    /// This policy applies to the content-security-policy header
    /// </summary>
    CspPolicy Csp { get; }
    /// <summary>
    /// The content security policy for report-only.
    /// This policy applies to the content-security-policy-report-only header
    /// </summary>
    CspPolicy CspReportOnly { get; }
    /// <summary>
    /// When set to to true, this group is applied throughout the
    /// app unless otherwise overriden with attributes or a filter. Defaults to true.
    /// It is mandatory to declare one default policy group
    /// </summary>
    bool IsDefault { get; set; }
    /// <summary>
    /// The number of bytes required to generate nonces server-side on each request.<br/>
    /// The default implementation <see cref="CspPolicyGroup"/> defaults it to 16.<br/>
    /// Recall that one byte can take up to 256=2^8 values (0 to 255).<br/>
    /// 16 bytes thus give 256^16=2^128 possibilities, or a 22 long webencoded base64 string.<br/>
    /// 8 gives 256^8=2^64 possibilities, or an 11 long base64 string, like Youtube.<br/>
    /// However, Youtube video ids have been argued to have 64^11 possibilities,
    /// which is 2^66 and thus not factorable into powers of 256.<br/>
    /// </summary>
    int NumberOfNonceBytes { get; set; }
}

}
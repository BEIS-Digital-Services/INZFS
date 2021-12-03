namespace Galebra.Security.Headers.Csp.Tools
{ 

/// <summary>
/// Common keywords used in content security policies
/// <see href="https://w3c.github.io/webappsec-csp/#csp-directives"/>
/// </summary>
public static class CspKeywords
{
    public const string None = "'none'";
    public const string ReportSample = "'report-sample'";
    public const string Self = "'self'";
    public const string StrictDynamic = "'strict-dynamic'";
    public const string UnsafeAllowRedirects = "'unsafe-allow-redirects'";
    public const string UnsafeEval = "'unsafe-eval'";
    public const string UnsafeHashes = "'unsafe-hashes'";
    public const string UnsafeInline = "'unsafe-inline'";

    public const string Nonce = "nonce";
    public const string Sha256 = "sha256";
    public const string Sha384 = "sha384";
    public const string Sha512 = "sha512";

    public const string Https = "https:";
    public const string Http = "http:";
    public const string Data = "data:";

    public const string Mediastream = "mediastream:";
    public const string Blob = "blob:";
    public const string FileSystem = "filesystem:";
}

}
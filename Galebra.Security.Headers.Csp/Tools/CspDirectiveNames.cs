namespace Galebra.Security.Headers.Csp.Tools
{ 

/// <summary>
/// <see href="https://w3c.github.io/webappsec-csp/#csp-directives"/>
/// </summary>
public static class CspDirectiveNames
{
    /// <summary>
    /// https://w3c.github.io/webappsec-csp/#directive-child-src
    /// </summary>
    public const string ChildSrc = "child-src";

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-csp/#directive-connect-src"/>
    /// </summary>
    public const string ConnectSrc = "connect-src";

    /// <summary>
    /// Default directive encapsulating a set of fetch directives.
    /// Should always be present in a policy. Possibly set it to <see cref="CspKeywords.None"/>.
    /// However, doing so does not set to none all directives,
    /// e.g. <see cref="CspDirectiveNames.FrameAncestors"/>.
    /// <see href="https://w3c.github.io/webappsec-csp/#directive-default-src"/>
    /// </summary>
    public const string DefaultSrc = "default-src";

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-csp/#directive-font-src"/>
    /// </summary>
    public const string FontSrc = "font-src";

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-csp/#directive-frame-src"/>
    /// </summary>
    public const string FrameSrc = "frame-src";

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-csp/#directive-img-src"/>
    /// </summary>
    public const string ImgSrc = "img-src";

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-csp/#directive-manifest-src"/>
    /// </summary>
    public const string ManifestSrc = "manifest-src";

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-csp/#directive-media-src"/>
    /// </summary>
    public const string MediaSrc = "media-src";

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-csp/#directive-prefetch-src"/>
    /// </summary>
    public const string PrefetchSrc = "prefetch-src";

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-csp/#directive-object-src"/>
    /// </summary>
    public const string ObjectSrc = "object-src";

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-csp/#directive-script-src"/>
    /// </summary>
    public const string ScriptSrc = "script-src";

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-csp/#directive-script-src-elem"/>
    /// </summary>
    public const string ScriptSrcElem = "script-src-elem";

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-csp/#directive-script-src-attr"/>
    /// </summary>
    public const string ScriptSrcAttr = "script-src-attr";

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-csp/#directive-style-src"/>
    /// </summary>
    public const string StyleSrc = "style-src";

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-csp/#directive-style-src-elem"/>
    /// </summary>
    public const string StyleSrcElem = "style-src-elem";

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-csp/#directive-style-src-attr"/>
    /// </summary>
    public const string StyleSrcAttr = "style-src-attr";

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-csp/#directive-worker-src"/>
    /// </summary>
    public const string WorkerSrc = "worker-src";

    ///Document directives

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-csp/#directive-base-uri"/>
    /// </summary>
    public const string BaseUri = "base-uri";

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-csp/#directive-sandbox"/>
    /// </summary>
    public const string Sandbox = "sandbox";

    ///Navigation directives

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-csp/#directive-form-action"/>
    /// </summary>
    public const string FormAction = "form-action";

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-csp/#directive-frame-ancestors"/>
    /// </summary>
    public const string FrameAncestors = "frame-ancestors";

    /// <summary>
    /// https://w3c.github.io/webappsec-csp/#directive-navigate-to
    /// </summary>
    public const string NavigateTo = "navigate-to";

    ///Reporting Directives

    /// <summary>
    /// https://w3c.github.io/webappsec-csp/#directive-report-uri
    /// </summary>
    public const string ReportUri = "report-uri";

    /// <summary>
    /// https://w3c.github.io/webappsec-csp/#directive-report-uri
    /// </summary>
    public const string ReportTo = "report-to";

    /// Other directives

    /// <summary>
    ///https://w3c.github.io/webappsec-csp/#directives-elsewhere
    /// </summary>
    public const string BlockAllMixedContent = "block-all-mixed-content";

    /// <summary>
    ///https://w3c.github.io/webappsec-csp/#directives-elsewhere
    /// </summary>
    public const string UpgradeInsecureRequests = "upgrade-insecure-requests";
}

}
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Galebra.Security.Headers.Csp.Infrastructure
{ 

/// <summary>
/// Middleware for Content Security Policy that efficiently deals with :<br/>
/// 1. A dictionary of policy groups <see cref="CspPolicyGroup"/><br/>
/// 2. Razor Pages Filters and attributes in pages, folders or controllers and actions,<br/>
/// see <see cref="IEnableCspFilterAttribute"/>, <see cref="IDisableCspFilterAttribute"/>
/// </summary>
public sealed class CspMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICspOptions _cspOptions;
    private readonly KeyValuePair<string, CspPolicyGroup> _defaultPolicyGroup;

    public CspMiddleware(RequestDelegate next, ICspOptions cspOptions)
    {
        _next = next;
        _cspOptions = cspOptions;
        //There should be only one default policy group at this stage
        //This group cannot be empty neither. So we have 3 cases
        //Fixed is empty, or Nonceable is empty or both are not empty
        _defaultPolicyGroup = _cspOptions.PolicyGroups.First(x => x.Value.IsDefault);
    }

    public Task Invoke(HttpContext context, ICspNonce cspNonce)
    {
        context.Response.OnStarting(() =>
        {
            if (context.Request.Path.StartsWithSegments("/admin")) { return Task.CompletedTask; }

            //context.Response.Headers.ContentSecurityPolicy requires the .Net 6
            //library to have access to the ASP.NET Core API 
            //https://github.com/dotnet/AspNetCore.Docs/issues/22048

            //Disable attribute or filter case
            var disableRequired = context.Items
            .TryGetValue(CspConstants.DisableCspResultFilterAttributeKey, out var disableObj);
            if (disableObj is bool enforceMode)
            {
                if (enforceMode)
                {
                    return Task.CompletedTask;
                }
                //enforceMode is false here, so let the pipeline continue
            }
            else
            {
                //enforceMode is null, which should never happen
                //since we always set it in attribute or filter
                //so let the pipeline continue
            }

            /// EnableCsp ResultFilter case
            /// Items is populated by the ResultFilterAttribute or Razor Pages Filter.
            /// obj can be null because null is allowed in <see cref="EnableCspAttribute"/>
            /// or <see cref="EnableCspPageFilter"/>,
            /// or because the attribute or filter is absent.
            if (context.Items.TryGetValue(CspConstants.EnableCspResultFilterAttributeKey, out var obj))
            {
                if (obj is string cspPolicyGroupName)
                {
                    SetCspHeaders(context.Response.Headers,
                        _cspOptions.PolicyGroups.First(kvp => kvp.Key == cspPolicyGroupName), cspNonce);
                }
                else
                {
                    // we have a null cspPolicyGroupName whereas the EnableCspResultFilterAttributeKey is
                    // present, so this is an attribute or filter with a null parameter (default), e.g. [EnableCsp]
                    SetCspHeaders(context.Response.Headers, _defaultPolicyGroup, cspNonce);
                }
            }
            else
            {
                //EnableCspResultFilterAttributeKey not found, so use default, unless all policies are disabled
                if (!_cspOptions.IsDisabled)
                {
                    if (disableRequired)
                    {
                        // There is a disable attribute or filter, no matter the enforce mode,
                        // so do not set
                    }
                    else
                    {
                        SetCspHeaders(context.Response.Headers, _defaultPolicyGroup, cspNonce);
                    }
                }
                else //All policies are disabled
                {
                }
            }

            /// The following is an alternative method to items. namely, invoke the endpoint,
            /// but middleware should be placed after UseRouting if code is outside of OnStarting,
            /// otherwise context.GetEndpoint() is always null
            ////var endpoint = context.GetEndpoint();
            ////var cspDisableMetadata = endpoint?.Metadata.GetMetadata<IDisableCspAttribute>();
            ////if (cspDisableMetadata is not null)
            ////{
            ////    return Task.CompletedTask;
            ////}
            ////if (endpoint?.Metadata.GetMetadata<IEnableCspAttribute>() is IEnableCspAttribute enableCspAttribute)
            ////{
            ////    if (enableCspAttribute.PolicyGroupName is not null)
            ////    {
            ////        (context.Response.Headers.ContentSecurityPolicy,
            ////        context.Response.Headers.ContentSecurityPolicyReportOnly)
            ////        = BuildHeaderValue(_cspOptions.PolicyGroups
            ////        .First(kvp => kvp.Key == enableCspAttribute.PolicyGroupName), cspNonce);

            ////        return Task.CompletedTask;
            ////    }
            ////    //Continue
            ////}

            //For checks
            //foreach (var service in cspNonce.Nonces)
            //{
            //    context.Response.Headers.Add(service.Key, service.Value.GetNonce());
            //}
            return Task.CompletedTask;
        });

        //context.Response.OnCompleted(() =>
        //{
        //    //var headerValue = context.Response.Headers.ContentSecurityPolicy;
        //    return Task.CompletedTask;
        //});
        return _next(context);
    }

    private static void SetCspHeaders(IHeaderDictionary headers,
        KeyValuePair<string, CspPolicyGroup> kvpPolicyGroup,
        ICspNonce cspNonce)
    {
        /// If the header value is empty then IIS will skip it, but not Kestrel.
        /// https://github.com/dotnet/aspnetcore/issues/4754
        /// So we need to make a check unfortunately.
        /// We use calls to strings rather than StringValues and we do this
        /// twice rather than using tuples because performance is circa twice as better
        var cspHeaderValue = BuildHeaderValue(kvpPolicyGroup.Key, kvpPolicyGroup.Value.Csp, cspNonce);
        if (!string.IsNullOrWhiteSpace(cspHeaderValue))
        {
            headers["Content-Security-Policy"] = cspHeaderValue;
        }
        var cspReportOnlyHeaderValue = BuildHeaderValue(kvpPolicyGroup.Key, kvpPolicyGroup.Value.CspReportOnly, cspNonce);
        if (!string.IsNullOrWhiteSpace(cspReportOnlyHeaderValue))
        {
            headers["Content-Security-Policy-Report-Only"] = cspReportOnlyHeaderValue;
        }
    }

    private static string
        BuildHeaderValue(string policyName, CspPolicy cspPolicy, ICspNonce cspNonce)
    {
        if (cspPolicy.RequiresNonce)
        {
            return string.Concat(cspPolicy.Fixed, string.Join(';',
                cspPolicy.Nonceable.ToList()
                .ConvertAll(s => s + cspNonce.Nonces[policyName].NonceHeader)));
        }
        return cspPolicy.Fixed;
    }
}

}
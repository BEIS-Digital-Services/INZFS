using Galebra.Security.Headers.Csp.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Galebra.Security.Headers.Csp
{ 

/// <summary>
/// <inheritdoc cref="IDisableCspFilterAttribute"/>.<br/>
/// <see cref="EnforceMode"/> defaults to true
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class DisableCspAttribute : ResultFilterAttribute, IDisableCspFilterAttribute
{
    /// <summary>
    /// <inheritdoc/>.<br/>
    /// Defaults to true
    /// </summary>
    public bool EnforceMode { get; init; } = true;

    public override void OnResultExecuting(ResultExecutingContext context)
    {
        //Set the EnforceMode as an Item object to be retrieved in the middleware
        context.HttpContext.Items[CspConstants.DisableCspResultFilterAttributeKey] = EnforceMode;
        base.OnResultExecuting(context);
    }
}

}
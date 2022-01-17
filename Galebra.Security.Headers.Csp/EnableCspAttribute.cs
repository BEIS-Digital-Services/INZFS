using Galebra.Security.Headers.Csp.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace Galebra.Security.Headers.Csp
{ 

/// <summary>
/// <inheritdoc cref="IEnableCspFilterAttribute"/><br/>
/// Defaults to the default <see cref="CspPolicyGroup"/>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class EnableCspAttribute : ResultFilterAttribute, IEnableCspFilterAttribute
{
    /// <summary>
    /// <inheritdoc cref="EnableCspAttribute"/>
    /// </summary>
    public EnableCspAttribute()
    {
    }
    public string? PolicyGroupName { get; init; }

    public override Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        return base.OnResultExecutionAsync(context, next);
    }

    public override void OnResultExecuting(ResultExecutingContext context)
    {
        /// One could override filter with an if statment, however this is not proper, especially that
        /// filter execute differently if they are requested globally or per folder.
        //if (context.HttpContext.Items[CspConstants.EnableCspResultFilterAttributeKey] is not null)
        //Set the PolicyGroupName as an Item object to be retrieved in the middleware
        context.HttpContext.Items[CspConstants.EnableCspResultFilterAttributeKey] = PolicyGroupName;
        base.OnResultExecuting(context);
    }
    public override void OnResultExecuted(ResultExecutedContext context)
    {
        base.OnResultExecuted(context);
    }
}

}
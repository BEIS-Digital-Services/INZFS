using Galebra.Security.Headers.Csp.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Galebra.Security.Headers.Csp
{ 

/// <summary>
/// <inheritdoc cref="IEnableCspFilterAttribute"/><br/>
/// Defaults to the default <see cref="CspPolicyGroup"/>
/// </summary>
public sealed class EnableCspPageFilter : IPageFilter, IEnableCspFilterAttribute
{
    public string? PolicyGroupName { get; init; }

    public void OnPageHandlerSelected(PageHandlerSelectedContext context)
    {
        context.HttpContext.Items[CspConstants.EnableCspResultFilterAttributeKey] = PolicyGroupName;
    }
    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
    }
    public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
    {
    }
}

}
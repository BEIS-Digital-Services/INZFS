using Galebra.Security.Headers.Csp.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Galebra.Security.Headers.Csp
{
    /// <summary>
    /// <inheritdoc cref="IDisableCspFilterAttribute"/>.<br/>
    /// <see cref="EnforceMode"/> defaults to true
    /// </summary>
    public sealed class DisableCspPageFilter : IPageFilter, IDisableCspFilterAttribute
    {
        /// <summary>
        /// <inheritdoc cref="DisableCspPageFilter"/>
        /// </summary>
        public DisableCspPageFilter()
        {

        }
        /// <summary>
        /// <inheritdoc/>.<br/>
        /// Defaults to true
        /// </summary>
        public bool EnforceMode { get; init; } = true;

        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
            context.HttpContext.Items[CspConstants.DisableCspResultFilterAttributeKey] = EnforceMode;
        }
        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
        }
        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
        }
    }
}

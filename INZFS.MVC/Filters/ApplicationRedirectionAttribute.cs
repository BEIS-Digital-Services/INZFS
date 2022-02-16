using INZFS.MVC.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using System;

namespace INZFS.MVC.Filters
{
    public class ApplicationRedirectionAttribute : ActionFilterAttribute
    {
        private readonly ApplicationOption _applicationOption;

        public ApplicationRedirectionAttribute(IOptions<ApplicationOption> applicationOption)
        {
            _applicationOption = applicationOption.Value;
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if(context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                if (DateTime.UtcNow > _applicationOption.EndDate)
                {
                    var values = new RouteValueDictionary(new
                    {
                        action = "ApplicationSent",
                        controller = "FundApplication"
                    });
                    context.Result = new RedirectToRouteResult(values);
                }
            }
            base.OnResultExecuting(context);
        }
    }
}

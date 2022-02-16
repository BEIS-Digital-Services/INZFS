using System;

namespace Microsoft.AspNetCore.Mvc
{
    public static class UrlHelperExtensions
    {
        public static string GetQuery(this IUrlHelper helper, string key)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            return helper.ActionContext?.HttpContext?.Request?.Query[key]?? string.Empty;
        }
    }
}

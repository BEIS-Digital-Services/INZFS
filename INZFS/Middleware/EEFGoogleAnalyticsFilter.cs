using Galebra.Security.Headers.Csp;
using Galebra.Security.Headers.Csp.Infrastructure;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.Admin;
using OrchardCore.Entities;
using OrchardCore.Google.Analytics.Settings;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INZFS.Web.Middleware
{
    public class EEFGoogleAnalyticsFilter : IAsyncResultFilter, IFilterMetadata
    {
        private readonly IResourceManager _resourceManager;
        private readonly ISiteService _siteService;
        private readonly ICspOptions _cspOptions;
        private readonly ICspNonce _cspNonce;
        private HtmlString _scriptsCache;

        public EEFGoogleAnalyticsFilter(IResourceManager resourceManager, ISiteService siteService, ICspOptions cspOptions, ICspNonce cspNonce)
        {
            this._resourceManager = resourceManager;
            this._siteService = siteService;
            this._cspOptions = cspOptions;
            this._cspNonce = cspNonce;
            _cspNonce.GenerateNonces();
        }

        public async Task OnResultExecutionAsync(
          ResultExecutingContext context,
          ResultExecutionDelegate next)
        {
            EEFGoogleAnalyticsFilter googleAnalyticsFilter = this;
            if ((context.Result is ViewResult || context.Result is PageResult) && !AdminAttribute.IsApplied(context.HttpContext))
            {
                if (googleAnalyticsFilter._scriptsCache == null)
                {
                    string sNonce = _cspNonce.Nonces["PolicyGroup1"].Nonce;

                    GoogleAnalyticsSettings analyticsSettings = (await googleAnalyticsFilter._siteService.GetSiteSettingsAsync()).As<GoogleAnalyticsSettings>();
                    if (!string.IsNullOrWhiteSpace(analyticsSettings?.TrackingID))
                        googleAnalyticsFilter._scriptsCache = new HtmlString("<!-- Global site tag (gtag.js) - Google Analytics -->\n<script async src=\"https://www.googletagmanager.com/gtag/js?id=" + analyticsSettings.TrackingID + "\"></script>\n<script nonce=\"" + sNonce + "\">window.dataLayer = window.dataLayer || [];function gtag() { dataLayer.push(arguments); }gtag('js', new Date());gtag('config', '" + analyticsSettings.TrackingID + "')</script>\n<!-- End Global site tag (gtag.js) - Google Analytics -->");
                }
                if (googleAnalyticsFilter._scriptsCache != null)
                    googleAnalyticsFilter._resourceManager.RegisterFootScript((IHtmlContent)googleAnalyticsFilter._scriptsCache);
            }
            ResultExecutedContext resultExecutedContext = await next();
        }
    }
}

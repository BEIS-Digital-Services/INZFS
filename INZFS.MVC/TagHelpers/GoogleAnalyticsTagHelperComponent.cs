using INZFS.MVC.Settings;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using System;

namespace INZFS.MVC.TagHelpers
{
    public class GoogleAnalyticsTagHelperComponent : TagHelperComponent
    {
        private readonly GoogleAnalyticsOptions _googleAnalyticsOptions;

        public GoogleAnalyticsTagHelperComponent(IOptions<GoogleAnalyticsOptions> googleAnalyticsOptions)
        {
            _googleAnalyticsOptions = googleAnalyticsOptions.Value;
        }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if(string.Equals(output.TagName, "head", StringComparison.OrdinalIgnoreCase))
            
            if (string.Equals(output.TagName, "head", StringComparison.OrdinalIgnoreCase))
            { 
                var trackingCode = _googleAnalyticsOptions.TrackingCode;
                if (!string.IsNullOrEmpty(trackingCode))
                {
                  
                    output.PostContent
                        .AppendHtml("<script src='https://www.googletagmanager.com/gtag/js?id=")
                        .AppendHtml(trackingCode)
                        .AppendHtml("'></script><script>window.dataLayer=window.dataLayer||[];function gtag(){dataLayer.push(arguments)}gtag('js',new Date);gtag('config','")
                        .AppendHtml(trackingCode)
                        .AppendHtml("',{displayFeaturesTask:'null'});</script>");
                }
            }
        }
    }
}

﻿using INZFS.MVC.Settings;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            // Inject the code only in the head element
            if (string.Equals(output.TagName, "head", StringComparison.OrdinalIgnoreCase))
            {
                // Get the tracking code from the configuration
                var trackingCode = _googleAnalyticsOptions.TrackingCode;
                if (!string.IsNullOrEmpty(trackingCode))
                {
                    // PostContent correspond to the text just before closing tag
                    output.PostContent
                        .AppendHtml("<script async src='https://www.googletagmanager.com/gtag/js?id=")
                        .AppendHtml(trackingCode)
                        .AppendHtml("'></script><script>window.dataLayer=window.dataLayer||[];function gtag(){dataLayer.push(arguments)}gtag('js',new Date);gtag('config','")
                        .AppendHtml(trackingCode)
                        .AppendHtml("',{displayFeaturesTask:'null'});</script>");
                }
            }
        }
    }
}

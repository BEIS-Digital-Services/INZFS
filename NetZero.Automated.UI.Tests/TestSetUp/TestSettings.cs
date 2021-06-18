using System;
using BEIS.GDS.UI.Tests.Support.Driver;
using NetZero.Automated.UI.Tests.Utils;

namespace NetZero.Automated.UI.Tests.TestSetUp
{
    public static class TestSettings 
    {

        // Create a new options instance, copy of the share, to use just in the current test, modifications in test will not affect other tests
        public static BrowserOptions Options => new BrowserOptions
        {
            BrowserType = (BrowserType)Enum.Parse(typeof(BrowserType), ConfigurationSetUp.BrowserType),
            FireEvents = false,
            Headless = bool.Parse(ConfigurationSetUp.Headless),
            IsDesktop = bool.Parse(ConfigurationSetUp.IsDesktop),
            DeviceName = ConfigurationSetUp.DeviceName,
            UserAgent = false,
            DefaultThinkTime = 2000,
            UCITestMode = true,
            UCIPerformanceMode = true,
            DisableExtensions = false,
            DisableFeatures = false,
            DisablePopupBlocking = false,
            DisableSettingsWindow = false,
            EnableJavascript = false,
            NoSandbox = false,
            DisableGpu = false,
            DumpDom = false,
            EnableAutomation = false,
            DisableImplSidePainting = false,
            StartMaximized = true,
            DisableDevShmUsage = false,
            DisableInfoBars = false,
            TestTypeBrowser = false,
            Height = null,
            Width = null,
            UseAutomationExtension = true,
            EnableAutomationExludeArg = true
        };
    }
}

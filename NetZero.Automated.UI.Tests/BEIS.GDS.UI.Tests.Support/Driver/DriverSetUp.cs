using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.Events;

namespace BEIS.GDS.UI.Tests.Support.Driver
{
    public class DriverSetUp
    {
        public IWebDriver SetUpDriver(BrowserOptions options)
        {
            IWebDriver driver;

            switch (options.BrowserType)
            {
                case BrowserType.Chrome:
                    if (options.IsDesktop)
                    {
                        var chromeService = ChromeDriverService.CreateDefaultService(options.DriversPath);
                        chromeService.HideCommandPromptWindow = options.HideDiagnosticWindow;
                        driver = new ChromeDriver(chromeService, options.ToChrome());
                        driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 20);
                    }
                    else
                    {
                        ChromeOptions chromeOptions = new ChromeOptions();
                        chromeOptions.EnableMobileEmulation(options.DeviceName);
                        return new ChromeDriver(chromeOptions);
                    }
                    break;
                case BrowserType.IE:
                    var ieService = InternetExplorerDriverService.CreateDefaultService(options.DriversPath);
                    ieService.SuppressInitialDiagnosticInformation = options.HideDiagnosticWindow;
                    driver = new InternetExplorerDriver(ieService, options.ToInternetExplorer(), TimeSpan.FromMinutes(20));
                    
                    break;
                case BrowserType.Firefox:
                    var ffService = FirefoxDriverService.CreateDefaultService(options.DriversPath);
                    ffService.HideCommandPromptWindow = options.HideDiagnosticWindow;
                    driver = new FirefoxDriver(ffService);
                    driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 5);
                    
                    break;
                case BrowserType.Edge:
                    var edgeService = EdgeDriverService.CreateDefaultService(options.DriversPath);
                    edgeService.HideCommandPromptWindow = options.HideDiagnosticWindow;
                    driver = new EdgeDriver(edgeService, options.ToEdge(), TimeSpan.FromMinutes(20));
                    
                    break;
                case BrowserType.Remote:
                    ICapabilities capabilities = null;
                    switch (options.RemoteBrowserType)
                    {
                        case BrowserType.Chrome:
                            capabilities = options.ToChrome().ToCapabilities();
                            break;
                        case BrowserType.Firefox:
                            capabilities = options.ToFireFox().ToCapabilities();
                            break;
                    }
                    driver = new RemoteWebDriver(options.RemoteHubServer, capabilities, TimeSpan.FromMinutes(20));
                    
                    break;
                default:
                    throw new InvalidOperationException(
                        $"The browser type '{options.BrowserType}' is not recognized.");
            }

            driver.Manage().Timeouts().PageLoad = options.PageLoadTimeout;

            // StartMaximized overrides a set width & height
            if (options.StartMaximized && options.BrowserType != BrowserType.Chrome) //Handle Chrome in the Browser Options
                driver.Manage().Window.Maximize();
            //else if (!options.StartMaximized && options.Width.HasValue && options.Height.HasValue)
            //    driver.Manage().Window.Size = new System.Drawing.Size(options.Width.Value, options.Height.Value);

            if (options.FireEvents || options.EnableRecording)
            {
                // Wrap the newly created driver.
                driver = new EventFiringWebDriver(driver);
            }

            return driver;
        }
    }
}

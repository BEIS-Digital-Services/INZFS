using Microsoft.Extensions.Configuration;
using System;

namespace NetZero.Automated.UI.Tests.Utils
{
    public class ConfigurationSetUp
    {
        
        private static IConfiguration config = GetConfigurations();
        public static string BaseUrl = config["NetZero:Environment"];
        public static readonly string BrowserType = config["Browser:Type"];
        public static readonly string Headless = config["Browser:Headless"];
        public static readonly string IsDesktop = config["Browser:IsDesktop"];
        public static readonly string DeviceName = config["Browser:DeviceName"];
        public static readonly string AdminUser = config["AdminUser:Username"];
        public static readonly string AdminPassword = config["AdminUser:Password"];
        public static readonly string Access = config["AdminUser:AccessToken"];
        public static readonly string RequestToken = config["AdminUser:RequestToken"];
        public static readonly string TestUser = config["TestUser:Username"];
        public static readonly string TestPassword = config["TestUser:Password"];
        private static IConfiguration GetConfigurations()
        {
            return new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        }
    }
}

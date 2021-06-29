using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading;

namespace INZFS.UnitTests
{
    class SeleniumTest
    {
        [Test]
        [Category("UITests")]
        public void VisitMicrosoft_CheckWindowsMenu()
        {
            IWebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl("https://inzfs-lab.london.cloudapps.digital/");
            Thread.Sleep(10000);
            string Windows_text = driver.FindElement(By.Id("shellmenu_1")).Text;
            Assert.AreEqual("StartNow", Windows_text);
            driver.Quit();
        }
    }
}
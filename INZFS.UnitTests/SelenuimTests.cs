using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading;

namespace INZFS.UnitTests
{
    class SeleniumTest
    {
        string labEnvironment = "https://inzfs-lab.london.cloudapps.digital/";
        string login = "//a[@href='/FundApplication/section/application-summary']";
        string submit = "//button[@type='submit']";

        [Test]
        [Category("UITests")]
        public void CheckLoginPage()
        {
            IWebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl(labEnvironment);
            Thread.Sleep(3000);
            driver.FindElement(By.XPath(login)).Click();
            driver.FindElement(By.Id("UserName")).SendKeys("Tester");
            driver.FindElement(By.Id("Password")).SendKeys("Tester");
            driver.FindElement(By.XPath(submit)).Click();
            Assert.That(driver.PageSource.Contains("Invalid login attempt."));
            driver.Quit();
        }
    }
}
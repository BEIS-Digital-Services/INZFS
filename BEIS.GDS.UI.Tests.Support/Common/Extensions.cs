using System;
using System.Collections.ObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;

namespace BEIS.GDS.UI.Tests.Support.Common
{
    public static class Extensions
    {
        public static void Check(this IWebElement element)
        {
            if (!element.Selected)
                element.Click();

            if (!element.Selected)
                throw new ElementNotInteractableException("Element was not clicked");
        }

        public static void UnCheck(this IWebElement element)
        {
            if (element.Selected)
                element.Click();
        }

        public static void Open(this IWebElement element)
        {
            if (element.GetAttribute("open") == null)
                element.FindElement(By.TagName("summary")).Click();
        }

        public static void Option(this IWebElement element, string byText)
        {
            SelectElement el = new SelectElement(element);
            el.SelectByText(byText);
        }

        public static void Option(this IWebElement element, int byIndex)
        {
            SelectElement el = new SelectElement(element);
            el.SelectByIndex(byIndex);
        }

        public static void ClearAndEnter(this IWebElement element, string text)
        {
           element.Clear();
           element.SendKeys(text);
        }

        public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv => drv.FindElement(by));
            }
            return driver.FindElement(by);
        }

        public static void EnterDate(this IDictionary<string, IWebElement> dateDict, DateTime date)
        {
            dateDict["Day"].ClearAndEnter(date.ToString("dd"));
            dateDict["Month"].ClearAndEnter(date.ToString("MM"));
            dateDict["Year"].ClearAndEnter(date.ToString("yyyy"));
        }


        public static ReadOnlyCollection<IWebElement> FindElements(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv => (drv.FindElements(by).Count > 0) ? drv.FindElements(by) : null);
            }
            return driver.FindElements(by);
        }

        public static string ToYesNoString(this bool value)
        {
            return value ? "Yes" : "No";
        }
    }
}

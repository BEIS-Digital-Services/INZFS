using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;

namespace BEIS.GDS.UI.Tests.Support.Common
{
    public class Components
    {
        private IWebDriver driver;
        public Components(IWebDriver driver)
        {
            this.driver = driver;
        }

        public IWebElement TextInput(string textInputId) => driver.FindElement(By.CssSelector($"input#{textInputId}.govuk-input"));
        public IWebElement DateInputDay(string dateInputDayId) => driver.FindElement(By.Id($"{dateInputDayId}-day"));
        public IWebElement DateInputMonth(string dateInputMonthId) => driver.FindElement(By.Id($"{dateInputMonthId}-day"));
        public IWebElement DateInputYear(string dateInputYearId) => driver.FindElement(By.Id($"{dateInputYearId}-day"));
        public IWebElement FileUpload => driver.FindElement(By.Id("file-upload-1"));
        public IWebElement ErrorMessage(string errorName) => driver.FindElement(By.Id($"{errorName}-error"));
        public IReadOnlyCollection<IWebElement> ErrorSummary => driver.FindElements(By.CssSelector("ul.govuk-list.govuk-error-summary__list > li"));
        public IReadOnlyCollection<IWebElement> Checkboxes => driver.FindElements(By.CssSelector("div.govuk-checkboxes > div > input"));
        public IWebElement Panel => driver.FindElement(By.CssSelector("div.govuk-panel.govuk-panel--confirmation"));
        public IWebElement Select(string dropdownId) => driver.FindElement(By.Id(dropdownId));

        private IWebElement SummaryListRowForKey(string key)
        {
            return driver.FindElements(By.CssSelector("div.govuk-summary-list__row"))
                    .FirstOrDefault(x => x.FindElement(By.CssSelector("dt.govuk-summary-list__key")).Text.Contains(key, StringComparison.InvariantCultureIgnoreCase));
        }
        public string SummaryListValueForKey(string key)
        {
            return SummaryListRowForKey(key)
                    .FindElement(By.CssSelector("dd.govuk-summary-list__value")).Text;
        }

        public IWebElement SummaryListChangeLinkForKey(string key)
        {
            return SummaryListRowForKey(key)
                    .FindElement(By.CssSelector("dd.govuk-summary-list__actions > a"));
        }

        public IWebElement TextArea(string textAreaId) => driver.FindElement(By.CssSelector($"textarea#{textAreaId}.govuk-textarea"));

        public Dictionary<string, IWebElement> DateInput(string idText)
        {
            var dateInputs = driver.FindElement(By.Id(idText)).FindElements(By.TagName("input"));
            return new Dictionary<string, IWebElement>()
            {
                { "Day", dateInputs[0] },
                { "Month", dateInputs[1] },
                { "Year", dateInputs[2] }
            };
        }

        public IWebElement FieldSetElement(string textboxId)
        {
            var fs = driver.FindElement(By.CssSelector("fieldset.govuk-fieldset"));
            return fs.FindElement(By.CssSelector($"input#{textboxId}.govuk-input"));
        }

        public IWebElement Tabs(string tabText)
        {
            var tabs = driver.FindElements(By.CssSelector("ul.govuk-tabs__list > li > a.govuk-tabs__tab"));
            if (tabs.Count > 0)
                return tabs.FirstOrDefault(x => x.Text == tabText);
            throw new NoSuchElementException($"{tabText} not found");
        }
        public IWebElement Button(string buttonText)
        {
            var buttons = driver.FindElements(By.CssSelector("button.govuk-button"), 10).Where(btn => btn.Text.Trim().ToLower() == buttonText.ToLower()).ToList();
            if (buttons.Count > 0)
                return buttons.FirstOrDefault();
            throw new NoSuchElementException($"{buttonText} button not found");
        }

        public IWebElement Checkbox(string checkboxLabel)
        {
            var boxes = driver.FindElements(By.CssSelector("div.govuk-checkboxes__item"));
            if (boxes.Count > 0)
                return boxes.FirstOrDefault(x => x.FindElement(By.TagName("label")).Text.Contains(checkboxLabel, StringComparison.InvariantCultureIgnoreCase))?.FindElement(By.TagName("input"));
            throw new NoSuchElementException($"{checkboxLabel} not found");
        }
        public IWebElement Radio(string radioLabel)
        {
            var radios = driver.FindElements(By.CssSelector("div.govuk-radios__item"));
            if (radios.Count > 0)
                return radios.FirstOrDefault(x => x.FindElement(By.TagName("label")).Text.Contains(radioLabel, StringComparison.InvariantCultureIgnoreCase))?.FindElement(By.TagName("input"));
            throw new NoSuchElementException($"{radioLabel} not found");
        }



        public IWebElement Details(string text)
        {
            var element = driver.FindElements(By.CssSelector("details.govuk-details")).FirstOrDefault(x => x.Text.Trim() == text);
            element.Open();
            return element.FindElement(By.CssSelector("div.govuk-details__text"));


        }
    }
}

using NetZero.Automated.UI.Tests.Utils;
using System;
using BEIS.GDS.UI.Tests.Support.Common;
using OpenQA.Selenium.Support.UI;
using TechTalk.SpecFlow;
using OpenQA.Selenium;
using FluentAssertions;
using System.Linq;
using System.Collections.Generic;

namespace NetZero.Automated.UI.Tests.Pages
{
    public abstract class BasePage 
    {
        public IWebDriver Driver;

        public Components Comp;

        public readonly string BaseUrl = ConfigurationSetUp.BaseUrl;
        protected WebDriverWait Wait => new WebDriverWait(Driver, TimeSpan.FromSeconds(20));
        IWebElement NewApplicationButton => Driver.FindElement(By.CssSelector("a.govuk-button.govuk-\\!-margin-top-6"), 10);

        private ScenarioContext context;
        protected BasePage(ScenarioContext context)
        {
            this.context = context;
            Driver = context.Get<IWebDriver>();
            Comp = new Components(Driver);
        }

        internal IWebElement ValidationSummary => Driver.FindElement(By.Id("validationSummary"));

        internal IWebElement FieldValidation(string field)
        {
            return Driver.FindElements(By.CssSelector("span.govuk-error-message.field-validation-error"))
                .First(x => x.GetAttribute("data-valmsg-for").ToLower() == field.ToLower());
        }

        public void WaitForPageToLoad()
        {
            Wait.Until(x => NewApplicationButton.Displayed);
        }        

        public void VerifyPageIsDirectedToHomepage()
        {
            WaitForPageToLoad();
            Driver.Url.Should().Be(BaseUrl+"/");
        }
    }
}

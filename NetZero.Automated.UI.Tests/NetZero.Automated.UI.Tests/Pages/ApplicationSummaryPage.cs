using BEIS.GDS.UI.Tests.Support.Common;
using FluentAssertions;
using OpenQA.Selenium;
using System;
using System.Linq;
using TechTalk.SpecFlow;
using static NetZero.Automated.UI.Tests.TestData.ApplicationData;

namespace NetZero.Automated.UI.Tests.Pages
{
    public class ApplicationSummaryPage : BasePage
    {
        ScenarioContext context;
        readonly string applicationSummaryUrl;

        IWebElement FinanceBarrierStatus => Driver.FindElement(By.Id("finance-barriers"));
        IWebElement FormSectionLink(string sectionName) => Driver.FindElement(By.LinkText(sectionName));
        IWebElement ContinueButton => Driver.FindElement(By.CssSelector("button.btn.btn-success.publish.govuk-button"), 10);
        public ApplicationSummaryPage(ScenarioContext context) : base (context)
        {
            this.context = context;
            applicationSummaryUrl = BaseUrl + "/FundApplication/section/application-summary";
        }

        private string FormSectionStatus(string sectionName)
        {
            var sectionRow = Driver.FindElements(By.CssSelector("li.app-task-list__item"))
                                .FirstOrDefault(r=>r.FindElement(By.CssSelector("span.app-task-list__task-name > a")).Text == sectionName);
            return sectionRow.FindElement(By.TagName("strong")).Text;
        }

        public void Open()
        {
            Driver.Navigate().GoToUrl(applicationSummaryUrl);
            Wait.Until(x => FinanceBarrierStatus.Displayed);
        }

        internal void ClickToOpenSection(string sectionName)
        {
            FormSectionLink(sectionName).Click();
            Wait.Until(x => ContinueButton.Displayed);
        }

        internal void VerifySectionCompletionStatus(string sectionName, string status)
        {
            FormSectionStatus(sectionName).Should().Be(status);
        }

        internal void VerifyDetailsEnteredInEachBasicInfoSection()
        {
            FormSectionLink("Company Details").Click();
            context.Get<BasicInformationInputPage>().VerifyDataOnCompanyDetailsPage(BasicInfo.CompanyName, BasicInfo.CompanyNumber);
            Open();
            FormSectionLink("Project Summary").Click();
            context.Get<BasicInformationInputPage>().VerifyDataOnProjectSummaryPage(BasicInfo.ProjectName, BasicInfo.TurnoverDate);
            Open();
            FormSectionLink("Project Details").Click();
            context.Get<BasicInformationInputPage>().VerifyDataOnProjectDetailsPage(BasicInfo.BriefProjectDescription, BasicInfo.ProjectCostEndBy2024);
            Open();
            FormSectionLink("Funding").Click();
            context.Get<BasicInformationInputPage>().VerifySelectionFundingPage(BasicInfo.CurrentFunding);
            Open();
        }

        internal void VerifyUpdatedDetailsInEachBasicInfoSection()
        {
            FormSectionLink("Company Details").Click();
            context.Get<BasicInformationInputPage>().VerifyDataOnCompanyDetailsPage(BasicInfo.UpdatedCompanyName, BasicInfo.UpdatedCompanyNumber);
            Open();
            FormSectionLink("Project Summary").Click();
            context.Get<BasicInformationInputPage>().VerifyDataOnProjectSummaryPage(BasicInfo.UpdatedProjectName, BasicInfo.UpdatedTurnoverDate);
            Open();
            FormSectionLink("Project Details").Click();
            context.Get<BasicInformationInputPage>().VerifyDataOnProjectDetailsPage(BasicInfo.UpdatedBriefProjectDescription, BasicInfo.UpdatedProjectCostEndBy2024);
            Open();
            FormSectionLink("Funding").Click();
            context.Get<BasicInformationInputPage>().VerifySelectionFundingPage(BasicInfo.UpdatedCurrentFunding);
            Open();
        }
    }
}

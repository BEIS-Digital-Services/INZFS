using BEIS.GDS.UI.Tests.Support.Common;
using FluentAssertions;
using NetZero.Automated.UI.Tests.Utils;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TechTalk.SpecFlow;
using static NetZero.Automated.UI.Tests.TestData.ApplicationData;

namespace NetZero.Automated.UI.Tests.Pages
{
    public class BasicInformationInputPage : BasePage
    {
        IWebElement CompanyName => Comp.TextInput("CompanyDetailsPart_CompanyName");
        IWebElement CompanyNumber => Comp.TextInput("CompanyDetailsPart_CompanyNumber");
        IWebElement ProjectName => Comp.TextInput("ProjectSummaryPart_ProjectName");
        IDictionary<string, IWebElement> TurnoverDate => Comp.DateInput("turn-over-date");
        IWebElement ProjectSummary => Comp.TextArea("with-hint");

        IWebElement SaveAndContinueButton => Driver.FindElement(By.CssSelector("a.govuk-button"), 10);
        IWebElement ContinueButton => Driver.FindElement(By.CssSelector("button.btn.btn-success.publish.govuk-button"), 10);
        IWebElement ProjectCostEndBy2024(bool value)
        {
            return Driver.FindElements(By.Id("ProjectDetailsPart_Timing"))?
                    .FirstOrDefault(x => x.GetAttribute("value") == Convert.ToInt32(value).ToString());
        }

        IWebElement UploadButton => Driver.FindElement(By.CssSelector("input.govuk-file-upload"), 10);
        IWebElement DownloadLink => Driver.FindElements(By.CssSelector("a.govuk-link"), 10).FirstOrDefault(l=>l.Text.Contains("Please Click here to download"));


        ScenarioContext context;
        readonly string basicInfoUrl;
        readonly string applicationSummaryUrl;
        readonly string uploadProjectPlanUrl;
        readonly string downloadsDirectory;

        static string testFile = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"TestData\TestFiles\TestPdf.pdf"));
        public BasicInformationInputPage(ScenarioContext context) : base (context)
        {
            this.context = context;
            basicInfoUrl = BaseUrl + "/FundApplication/section/project-summary";

            applicationSummaryUrl = BaseUrl + "/FundApplication/section/application-summary";
            uploadProjectPlanUrl = BaseUrl + "/FundApplication/section/upload-project-plan";
            downloadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "downloads");
        }

        public void Open()
        {
            Driver.Navigate().GoToUrl(basicInfoUrl);
            Wait.Until(x=> ProjectName.Displayed);
        }

        public void OpenSummaryPage()
        {
            Driver.Navigate().GoToUrl(applicationSummaryUrl);
            Wait.Until(x => SaveAndContinueButton.Displayed);
        }

        public bool IsThePageLoaded()
        {
            return ProjectName.Displayed;
        }

        internal void EnterValidBasicInformationInputs()
        {
            context.Get<ApplicationSummaryPage>().ClickToOpenSection("Company Details");
            EnterCompanyDetails(BasicInfo.CompanyName, BasicInfo.CompanyNumber, "Entering");
            ContinueToNextPage();
            EnterProjectSummary(BasicInfo.ProjectName, BasicInfo.TurnoverDate, "Entering");            
            ContinueToNextPage();            
            EnterProjectDetails(BasicInfo.BriefProjectDescription, BasicInfo.ProjectCostEndBy2024, "Entering");
            ContinueToNextPage();            
            SelectProjectFunding(BasicInfo.CurrentFunding, "Selecting");            
            ContinueToNextPage();
        }

        internal void UpdateBasicInformationInputs()
        {
            context.Get<ApplicationSummaryPage>().ClickToOpenSection("Company Details");
            EnterCompanyDetails(BasicInfo.UpdatedCompanyName, BasicInfo.UpdatedCompanyNumber, "Updating");
            ContinueToNextPage();
            EnterProjectSummary(BasicInfo.UpdatedProjectName, BasicInfo.UpdatedTurnoverDate, "Updating");
            ContinueToNextPage();
            EnterProjectDetails(BasicInfo.UpdatedBriefProjectDescription, BasicInfo.UpdatedProjectCostEndBy2024, "Updating");
            ContinueToNextPage();
            UncheckCheckboxes();
            SelectProjectFunding(BasicInfo.UpdatedCurrentFunding, "Updating");
            ContinueToNextPage();
        }

        private void SelectCurrentFundingCheckboxes(List<string> itemsToCheck)
        {
            foreach (var item in itemsToCheck)
            {
                Comp.Checkbox(item).Check();
            }
        }

        private void UncheckCheckboxes()
        {
            foreach (var checkbox in Comp.Checkboxes)
            {
                checkbox.UnCheck();
            }
        }

        internal void DownloadTheTestDocument()
        {
            OpenUploadProjectPlanPage();
            if (Directory.Exists(downloadsDirectory))
                Directory.Delete(downloadsDirectory, true);
            DownloadLink.Click();
            Wait.Until(f => Directory.Exists(downloadsDirectory));
            Wait.Until(f => DirectoryContainsFiles());
        }

        private bool DirectoryContainsFiles()
        {
            DirectoryInfo di = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(),"downloads" ));
            return di.GetFiles("*.pdf").Any();
        }

        internal void VerifyTestDocumentIsDownload()
        {
            DirectoryInfo di = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "downloads"));
            di.GetFiles("*.pdf").FirstOrDefault().Name.Should().Contain(testFile.Split('\\').Last());
            FileInfo file = new FileInfo(testFile);
            di.GetFiles("*.pdf").FirstOrDefault().Length.Should().Be(file.Length);
        }

        internal void VerifyUploadedDocument()
        {
            OpenUploadProjectPlanPage();
            DownloadLink.Displayed.Should().BeTrue();
            DownloadLink.GetAttribute("href").Should().EndWith(testFile.Split('\\').Last());
        }

        internal void UploadTestDocument()
        {
            UploadButton.SendKeys(testFile);
            ContinueToNextPage();
        }

        internal void OpenUploadProjectPlanPage()
        {
            Driver.Navigate().GoToUrl(uploadProjectPlanUrl);
            Wait.Until(x => UploadButton.Displayed);
        }

        internal void ContinueToNextPage() 
        {
            ContinueButton.Click();
        }
        internal void EnterCompanyDetails(string companyName, string companyNumber, string logMsg)
        {
            context.Get<Logger>().LogMessage($"{logMsg} company details...");
            CompanyName.ClearAndEnter(companyName);
            CompanyNumber.ClearAndEnter(companyNumber);
        }


        internal void EnterProjectDetails(string projectDescription, bool projCost, string logMsg)
        {
            context.Get<Logger>().LogMessage($"{logMsg} project details...");
            ProjectSummary.ClearAndEnter(projectDescription);
            ProjectCostEndBy2024(projCost).Click();
        }

        internal void EnterProjectSummary(string projectName, DateTime turnoverDate, string logMsg)
        {
            context.Get<Logger>().Log($"{logMsg} project summary details...");
            ProjectName.ClearAndEnter(projectName);
            TurnoverDate.EnterDate(turnoverDate);
        }

        internal void SelectProjectFunding(List<string> currentFunding, string logMsg)
        {
            context.Get<Logger>().LogMessage($"{logMsg} funding options...");
            SelectCurrentFundingCheckboxes(currentFunding);
        }

        internal void VerifyDataOnCompanyDetailsPage(string companyName, string companyNumber)
        {
            context.Get<Logger>().LogMessage("Verifying company details...");
            CompanyName.GetAttribute("value").Should().Be(companyName);
            CompanyNumber.GetAttribute("value").Should().Be(companyNumber);
        }
        internal void VerifyDataOnProjectSummaryPage(string projectName, DateTime turnoverDate)
        {
            context.Get<Logger>().LogMessage("Verifying project summary details...");
            ProjectName.GetAttribute("value").Should().Be(projectName);
            TurnoverDate["Day"].GetAttribute("value").Should().Be(turnoverDate.ToString("dd"));
            TurnoverDate["Month"].GetAttribute("value").Should().Be(turnoverDate.ToString("MM"));
            TurnoverDate["Year"].GetAttribute("value").Should().Be(turnoverDate.ToString("yyyy"));
        }
        internal void VerifyDataOnProjectDetailsPage(string projectDescription, bool projCost)
        {
            context.Get<Logger>().LogMessage("Verifying project details...");
            ProjectSummary.GetAttribute("value").Should().Be(projectDescription);
            ProjectCostEndBy2024(projCost).Selected.Should().BeTrue();
        }
        internal void VerifySelectionFundingPage(List<string> currentFunding)
        {
            context.Get<Logger>().LogMessage("Verifying current funding selection...");
            foreach (var item in currentFunding)
            {
                Comp.Checkbox(item).Selected.Should().BeTrue();
            }
        }

        internal void VerifyValidationSummaryMatches(string validations)
        {
            var errorList = Comp.ErrorSummary;
            List<string> errorTextList = new List<string>();
            foreach (var error in errorList)
            {
                errorTextList.Add(error.Text);
            }
            var errorSummary = String.Join(", ", errorTextList.ToArray());
            errorSummary.Should().Contain(validations);
        }
    }
}

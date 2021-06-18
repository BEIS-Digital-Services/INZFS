using System;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Gherkin.Model;
using BEIS.GDS.UI.Tests.Support.Driver;
using NetZero.Automated.UI.Tests.Helpers;
using NetZero.Automated.UI.Tests.Pages;
using NetZero.Automated.UI.Tests.Pages.CMS;
using NetZero.Automated.UI.Tests.TestSetUp;
using NUnit.Framework;
using OpenQA.Selenium;
using TechTalk.SpecFlow;

namespace NetZero.Automated.UI.Tests.Utils
{

    [Binding]
    public sealed class Hooks
    {
        private ScenarioContext context;
        
        private static AventStack.ExtentReports.ExtentReports extent;
        private static ExtentTest feature;
        private static ExtentTest scenario;
        public Hooks(ScenarioContext context)
        {
            this.context = context;
        }

        [BeforeTestRun]
        public static void ReportInitialize()
        {
            extent = new AventStack.ExtentReports.ExtentReports();
            extent.AttachReporter(ReportHelpers.InitializeAndGetReporter());
            ReportHelpers.TimeStamp = DateTime.Now.ToString("ddMMyy");
        }

        [BeforeFeature]
        public static void BeforeFeature(FeatureContext featureContext)
        {
            feature = extent.CreateTest<Feature>(featureContext.FeatureInfo.Title);
        }

        [BeforeScenario("UserCleanUp")]
        public void UserCleanUp()
        {
            context.Set(new LoginPage(context));
            context.Set(new UsersPage(context));
            context.Get<Logger>().Log("Test user clean up starting...");
            context.Get<UsersPage>().DeleteTestUsers();
            context.Get<Logger>().Log("Test user clean up done.");
        }

        [BeforeScenario(Order = 0)]
        public void BeforeScenario()
        {
            scenario = feature.CreateNode<Scenario>(context.ScenarioInfo.Title);
            context.Set(new DriverSetUp().SetUpDriver(TestSettings.Options));
            context.Get<IWebDriver>().Navigate().GoToUrl(ConfigurationSetUp.BaseUrl);
            context.Set(new Logger());
            context.Set(new ReportHelpers(context));
            context.Get<Logger>().Log($"Scenario: {context.ScenarioInfo.Title}");
            context.Get<Logger>().LogMessage($"---------------------------------------------------------");
        }


        [AfterScenario]
        public void AfterScenario(FeatureContext featureContext)
        {
            extent.Flush();
            context.Get<IWebDriver>().Quit();
            context.Get<ReportHelpers>().AttachTestArtifact();
            TestContext.AddTestAttachment(context.Get<Logger>().FilePath);
            context.Clear();
        }

        [BeforeStep]
        public void BeforeStep()
        {
            context.Get<Logger>().LogToFile($"__________________________________________________________________________");
            context.Get<Logger>().LogToFile($"\r\n{DateTime.Now.ToString("dd/MM/yy HH:mm:ss")} | LOG STEP NAME: {context.StepContext.StepInfo.StepDefinitionType} {context.StepContext.StepInfo.Text}");
        }

        [AfterStep]
        public void AfterStep()
        {
            var text = context.TestError == null ? $" --> done: {context.StepContext.StepInfo.Text}" : " --> ERROR :";
            context.Get<Logger>().LogToFile(text);
            context.Get<ReportHelpers>().InsertStepsInReport(feature, scenario);

        }
    }
}

using AventStack.ExtentReports;
using AventStack.ExtentReports.Gherkin.Model;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Configuration;
using NetZero.Automated.UI.Tests.Pages.CMS;
using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Tracing;
using WDSE;
using WDSE.Decorators;
using WDSE.ScreenshotMaker;

namespace NetZero.Automated.UI.Tests.Helpers
{
    public class ReportHelpers
    {
        private ScenarioContext context;
        public static string ReportFolder = Path.Combine(Directory.GetCurrentDirectory(), "testresults");
        public static string TimeStamp;
        private static string TestReportPath = Path.Combine(ReportFolder, $"NetZero_TestReport_{TimeStamp}.html");
        private string FailedTestArtifactName;

        public ReportHelpers(ScenarioContext context)
        {
            this.context = context;
        }
        public static ExtentHtmlReporter InitializeAndGetReporter()
        {
            var htmlReporter =
                new ExtentHtmlReporter(Path.Combine(ReportFolder, "index.html"));
            htmlReporter.Config.Theme = Theme.Dark;
            return htmlReporter;

        }

        private ExtentTest CreateStepNode(ExtentTest scenario, ScenarioStepContext stepContext)
        {
            var stepType = stepContext.StepInfo.StepDefinitionType.ToString();
            ExtentTest stepNode;
            switch (stepType)
            {
                case "Given":
                    stepNode = scenario.CreateNode<Given>($"<B><i>{stepType} </i></B>" + stepContext.StepInfo.Text);
                    break;
                case "When":
                    stepNode = scenario.CreateNode<When>($"<B><i>{stepType} </i></B>" + stepContext.StepInfo.Text);
                    break;
                case "Then":
                    stepNode = scenario.CreateNode<Then>($"<B><i>{stepType} </i></B>" + stepContext.StepInfo.Text);
                    break;
                default:
                    stepNode = scenario.CreateNode<And>($"<B><i>{stepType} </i></B>" + stepContext.StepInfo.Text);
                    break;
            }

            return stepNode;
        }

        public void InsertStepsInReport(ExtentTest feature, ExtentTest scenario)
        {
            PropertyInfo pInfo = typeof(ScenarioContext).GetProperty("ScenarioExecutionStatus", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo getter = pInfo.GetGetMethod(nonPublic: true);
            object TestResult = getter.Invoke(context, null);
            string details = string.Empty;
            if (context.StepContext.StepInfo.Table != null)
                details = $"<pre>{context.StepContext.StepInfo.Table.ToString().Replace(@"\r\n", "<br>")}</pre>";

            if (context.TestError != null)
            {
                FailedTestArtifactName = $"error_{feature.Model.Name}_{context.ScenarioInfo.Title.ToIdentifier()}";
                CapturePageSource(FailedTestArtifactName);
                var imageByteArr = CaptureScreenshot(FailedTestArtifactName);
                var media = ScreenshotModel(imageByteArr);
                CreateStepNode(scenario, ScenarioStepContext.Current).Fail(details + $"<br>{context.TestError.Message}<br>{context.TestError.StackTrace}", media);
            }
            else
            {
                CreateStepNode(scenario, ScenarioStepContext.Current).Pass(details);
            }

            if (TestResult.ToString() == "StepDefinitionPending")
            {
                CreateStepNode(scenario, ScenarioStepContext.Current).Skip("Step Definition Pending" + "<br>" + details);
            }
        }

        private byte[] CaptureScreenshot(string artifactName)
        {
            byte[] bytesArr = null;
            try
            {
                var screenshotFilePath = Path.Combine(ReportFolder, artifactName + "_screenshot.png");
                bytesArr = context.Get<IWebDriver>().TakeScreenshot(new VerticalCombineDecorator(new ScreenshotMaker()));
                File.WriteAllBytes(screenshotFilePath, bytesArr);
                Console.WriteLine("Screenshot: {0}", new Uri(screenshotFilePath));
                TestContext.AddTestAttachment(screenshotFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while taking screenshot: {0}", ex);
            }
            return bytesArr;
        }

        private void CapturePageSource(string artifactName)
        {
            string pageSource = context.Get<IWebDriver>().PageSource;
            string sourceFilePath = Path.Combine(ReportFolder, artifactName + "_source.html");
            File.WriteAllText(sourceFilePath, pageSource, Encoding.UTF8);
            Console.WriteLine("Page source: {0}", new Uri(sourceFilePath));
            TestContext.AddTestAttachment(sourceFilePath);
        }

        internal void AttachTestArtifact()
        {
            if (File.Exists(TestReportPath))
                File.Delete(TestReportPath);
            FileInfo file = new FileInfo(Path.Combine(ReportFolder, "index.html"));
            file.MoveTo(TestReportPath);
            TestContext.AddTestAttachment(TestReportPath);
        }

        private MediaEntityModelProvider ScreenshotModel(byte[] byteArr)
        {
            return MediaEntityBuilder.CreateScreenCaptureFromBase64String(Convert.ToBase64String(byteArr)).Build();
        }
        public class TestReport
        {
            [Test]
            public void GenerateReport()
            {
                TimeStamp = DateTime.Now.ToString("ddMMyy");
                TestContext.AddTestAttachment(TestReportPath);
                Assert.Pass("");

            }
        }
    }
}

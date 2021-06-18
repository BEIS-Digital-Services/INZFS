using NetZero.Automated.UI.Tests.Pages;
using TechTalk.SpecFlow;
using static NetZero.Automated.UI.Tests.TestData.ApplicationData;

namespace NetZero.Automated.UI.Tests.Steps
{
    [Binding]
    public sealed class ApplicationSteps
    {

        private readonly ScenarioContext scenarioContext;

        public ApplicationSteps(ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
            this.scenarioContext.Set(new BasicInformationInputPage(scenarioContext));
            this.scenarioContext.Set(new ApplicationSummaryPage(scenarioContext));
        }

        [When(@"I navigate back to the application summary page")]
        [Given(@"I navigate back to the application summary page")]
        [Given(@"I am on the Application Summary page")]
        public void GivenIAmOnTheApplicationSummarypage()
        {
            scenarioContext.Get<ApplicationSummaryPage>().Open();
        }

        [Given(@"I enter all the basic information details and save")]
        [When(@"I enter all the basic information details and save")]
        public void WhenIEnterAllTheBasicInformationDetails()
        {
            scenarioContext.Get<BasicInformationInputPage>().EnterValidBasicInformationInputs();
        }

        [Given(@"I am on the first basic info section page")]
        public void GivenIAmOnTheFirstBasicInfoSectionPage()
        {
            scenarioContext.Get<ApplicationSummaryPage>().ClickToOpenSection("Company Details");
        }


        [When(@"I update basic info details on each section")]
        public void WhenIClickChangeAndUpdateEachOfBasicInformationInput()
        {
            scenarioContext.Get<BasicInformationInputPage>().UpdateBasicInformationInputs();
        }

        [Then(@"upon visting each basic information section I should be able to see that the details are updated")]
        public void ThenOnTheSummaryPageTheUpdatedValuesAreShownAsExpected()
        {
            scenarioContext.Get<ApplicationSummaryPage>().VerifyUpdatedDetailsInEachBasicInfoSection();
        }

        [When(@"I click continue without filling any details on the Company details page")]
        [Then(@"I click continue without slecting any of the funding options")]
        [Then(@"I click continue without filling any details on the Project Summary page")]
        [Then(@"I click continue without filling any details on the Project Details page")]
        public void WhenIClickContinueWithoutFillingAnyDetailsOnTheProjectDetailsPage()
        {
            scenarioContext.Get<BasicInformationInputPage>().ContinueToNextPage();
        }

        [Then(@"I should see the validations ""(.*)""")]
        public void ThenIShouldSeeTheValidations(string validations)
        {
            scenarioContext.Get<BasicInformationInputPage>().VerifyValidationSummaryMatches(validations);
        }

        [Then(@"I enter valid company details and continue")]
        public void ThenIEnterValidCompanyDetailsAndContinue()
        {
            scenarioContext.Get<BasicInformationInputPage>()
                .EnterCompanyDetails(BasicInfo.CompanyName, BasicInfo.CompanyNumber, "Entering");
            scenarioContext.Get<BasicInformationInputPage>().ContinueToNextPage();
        }


        [Then(@"I enter valid project details and continue")]
        public void ThenIEnterValidProjectDetailsAndContinue()
        {
            scenarioContext.Get<BasicInformationInputPage>()
                .EnterProjectDetails(BasicInfo.BriefProjectDescription, BasicInfo.ProjectCostEndBy2024, "Entering");
            scenarioContext.Get<BasicInformationInputPage>().ContinueToNextPage();
        }

        [Then(@"I enter valid project introduction input and continue")]
        public void ThenIEnterValidProjectIntorductionInputAndContinue()
        {
            scenarioContext.Get<BasicInformationInputPage>()
                .EnterProjectSummary(BasicInfo.ProjectName, BasicInfo.TurnoverDate, "Entering");
            scenarioContext.Get<BasicInformationInputPage>().ContinueToNextPage();
        }

        

        [Given(@"I am on upload project plan page")]
        public void GivenIAmOnUploadProjectPlanPage()
        {
            scenarioContext.Get<BasicInformationInputPage>().OpenUploadProjectPlanPage();
        }

        [When(@"I upload a document")]
        public void WhenIUploadADocument()
        {
            scenarioContext.Get<BasicInformationInputPage>().UploadTestDocument();
        }

        [Then(@"I should see that the document is uploaded")]
        public void ThenIShouldSeeThatTheDocumentIsUploaded()
        {
            scenarioContext.Get<BasicInformationInputPage>().VerifyUploadedDocument();
        }

        [When(@"I click on the download document link")]
        public void WhenIClickOnTheDownloadDocumentLink()
        {
            scenarioContext.Get<BasicInformationInputPage>().DownloadTheTestDocument();
        }

        [Then(@"I should see that the document is downloaded")]
        public void ThenIShouldSeeThatTheDocumentIsDownloaded()
        {
            scenarioContext.Get<BasicInformationInputPage>().VerifyTestDocumentIsDownload();
        }

        [Then(@"I should see status next to each of the basic information section link as per below")]
        public void ThenIShouldSeeStatusNextToEachOfTheBasicInformationSectionLink(Table table)
        {
            foreach (var row in table.Rows)
            {
                scenarioContext.Get<ApplicationSummaryPage>()
                    .VerifySectionCompletionStatus(row["Section"], row["Status"]);
            }            
        }

        [Then(@"upon visting each basic information section I should be able to see that the details are populated")]
        public void ThenUponVistingEachBasicInformationSectionWeShouldBeAbleToSeeThatDeatilsArePopulated()
        {
            scenarioContext.Get<ApplicationSummaryPage>().VerifyDetailsEnteredInEachBasicInfoSection();
        }


    }
}

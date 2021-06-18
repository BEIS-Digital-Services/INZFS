using NetZero.Automated.UI.Tests.Pages;
using NetZero.Automated.UI.Tests.Utils;
using TechTalk.SpecFlow;

namespace NetZero.Automated.UI.Tests.Steps
{
    [Binding]
    public sealed class LoginSteps
    {
        private readonly ScenarioContext scenarioContext;

        public LoginSteps(ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
            this.scenarioContext.Set(new LoginPage(scenarioContext));
        }

        [Given(@"I am on the login page")]
        public void GivenTheUserIsOnTheLoginPage()
        {
            scenarioContext.Get<LoginPage>().Open();
        }

        [When(@"I submit valid login details")]
        public void WhenTheUserSubmitsValidLoginDetails()
        {
            scenarioContext.Get<LoginPage>().LoginTestUser();
        }

        [When(@"I submit invalid login details")]
        public void WhenTheUserSubmitsInvalidLoginDetails()
        {
            scenarioContext.Get<LoginPage>().LoginWithInvalidDetails();
        }

        [When(@"I submit with the below combination of field values then I should see the FieldValidation and ValidationSummary accordingly")]
        public void WhenISubmitWithTheBelowCombinationOfFieldValuesThenIShouldSeeTheFieldValidationAndValidationSummaryAccordingly(Table table)
        {
            scenarioContext.Get<Logger>().Log("Verifying field and summary level validations...");
            foreach (var row in table.Rows)
            {
                scenarioContext.Get<Logger>().LogMessage($"Entering Username '{row["UsernameValue"]}' and password '{row["PasswordValue"]}'...");
                scenarioContext.Get<LoginPage>().EnterLoginValuesAndSubmit(row["UsernameValue"], row["PasswordValue"]);
                scenarioContext.Get<LoginPage>().VerifyIncompleteLoginEntryValidation(row["UsernameValue"], row["PasswordValue"], row["FieldValidation"], row["ValidationSummary"]);
            }
        }

        [Then(@"I should see relevant error message")]
        public void ThenIShouldSeeRelevantErrorMessage()
        {
            scenarioContext.Get<LoginPage>().VerifyInvalidLoginValidation();
        }

        [Then(@"I should remain on the Login page")]
        public void ThenIShouldRemainOnTheLoginPage()
        {
            scenarioContext.Get<LoginPage>().VerifyLoginPageIsOpen();
        }

        [Then(@"I am able to access application page")]
        public void ThenIAmAbleToAccessApplicationPage()
        {
            scenarioContext.Get<LoginPage>().VerifyApplicationIsAccessible();
        }

        [Given(@"I have logged in with the new user details")]
        public void GivenIHaveLoggedIn()
        {
            scenarioContext.Get<LoginPage>().LoginNewUser();
        }


    }


}

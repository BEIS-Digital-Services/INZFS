using NetZero.Automated.UI.Tests.Pages;
using NetZero.Automated.UI.Tests.Utils;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace NetZero.Automated.UI.Tests.Steps
{
    [Binding]
    public sealed class RegistrationSteps
    {
        
        private readonly ScenarioContext scenarioContext;


        public RegistrationSteps(ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
            this.scenarioContext.Set(new RegistrationPage(scenarioContext));
        }

        [Given(@"I am on the Registration page")]
        public void GivenTheUserIsOnTheRegistrationPage()
        {
            scenarioContext.Get<RegistrationPage>().Open();
        }


        [When(@"I provide valid registration details")]
        public void WhenTheUserProvidesValidRegistrationDetails()
        {
            scenarioContext.Get<RegistrationPage>().EnterValidRegistrationDetails();
        }

        [When(@"I click on the submit button")]
        [When(@"I submit the registration")]
        public void WhenSubmitsTheRegistration()
        {
            scenarioContext.Get<RegistrationPage>().SubmitRegistration();
        }

        [When(@"I provide an existing user registration details")]
        public void WhenIProvideAnExistingUserRegistrationDetails()
        {
            scenarioContext.Get<RegistrationPage>().EnterExistingUserRegistrationDetails();
        }

        [When(@"I enter (.*) in (.*)")]
        public void WhenIEnterIn(string value, string field)
        {
            scenarioContext.Get<RegistrationPage>().EnterRegistrationValueInTheField(value, field);
        }


        [Then(@"I am able to see the confirmation message")]
        public void ThenTheUserIsAbleToSeeTheConfirmationMessage()
        {
            Assert.Fail("Failed on purpose to see how the failure would look like");
        }

        [Then(@"I should be directed to the home page")]
        public void ThenTheUserIsDirectedToTheLoginPage()
        {
            scenarioContext.Get<RegistrationPage>().VerifyPageIsDirectedToHomepage();
        }

        [Then(@"the user is created")]
        public void ThenTheUserIsCreated()
        {
            scenarioContext.Get<RegistrationPage>().VerifyTestUserIsCreated();
        }

        [Then(@"I should see validation messages accordingly")]
        public void ThenIShouldSeeValidationMessagesAccordingly()
        {
            scenarioContext.Get<RegistrationPage>().VerifyFieldValidations();
        }

        [Then(@"I should see (.*) validation messages (.*) and (.*) accordingly")]
        public void ThenIShouldSeeValidationMessagesAndAccordingly(string field, string fieldValidation, string summaryValidation)
        {
            scenarioContext.Get<RegistrationPage>().VerifyFieldAndSummaryValidations(field, fieldValidation, summaryValidation);
        }

        [When(@"I submit with the below Fields and its InvalidValue then I should see the FieldValidation and ValidationSummary accordingly")]
        public void WhenISubmitWithTheBelowFieldsAndItsInvalidValueThenIShouldSeeTheFieldValidationAndValidationSummaryAccordingly(Table table)
        {
            scenarioContext.Get<Logger>().Log("Verifying field and summary level validations...");
            foreach (var row in table.Rows)
            {
                scenarioContext.Get<Logger>().LogMessage($"Entering '{row["InvalidValue"]}' in the field '{row["Field"]}'...");
                scenarioContext.Get<RegistrationPage>().EnterValidRegistrationDetails();
                scenarioContext.Get<RegistrationPage>().EnterRegistrationValueInTheField(row["InvalidValue"], row["Field"]);
                scenarioContext.Get<RegistrationPage>().SubmitRegistration();
                scenarioContext.Get<RegistrationPage>().VerifyFieldAndSummaryValidations(row["Field"], row["FieldValidation"], row["ValidationSummary"]);
            }
            
        }


        [When(@"I submit with valid but different passwords")]
        public void WhenIProvideValidButDifferentPassword()
        {
            scenarioContext.Get<RegistrationPage>().EnterValidButDifferentPassword();
            scenarioContext.Get<RegistrationPage>().SubmitRegistration();
        }

        [Then(@"I should see password mismatch validation")]
        public void ThenIShouldSeePasswordMismatchValidation()
        {
            scenarioContext.Get<RegistrationPage>().VerifyPasswordMismatchValidation();
        }

        [Given(@"I have registered a new user")]
        public void GivenIHaveRegisteredMyself()
        {
            scenarioContext.Get<RegistrationPage>().RegisterNewTestApplicant();
        }


    }
}

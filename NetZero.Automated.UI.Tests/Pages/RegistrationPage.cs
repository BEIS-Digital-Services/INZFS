using NetZero.Automated.UI.Tests.Pages.CMS;
using NetZero.Automated.UI.Tests.TestData;
using OpenQA.Selenium;
using System;
using BEIS.GDS.UI.Tests.Support.Common;
using FluentAssertions;
using TechTalk.SpecFlow;
using NetZero.Automated.UI.Tests.Utils;

namespace NetZero.Automated.UI.Tests.Pages
{
    public class RegistrationPage : BasePage
    {
        private readonly string registerUrl;

        private readonly ScenarioContext context;
        public RegistrationPage(ScenarioContext context) : base(context)
        {
            registerUrl = BaseUrl + "/register";
            this.context = context;
        }

        private readonly string usernameValidation = "Username is required.";
        private readonly string emailValidation = "Email is required.";
        private readonly string passwordValidation = "Password is required.";
        private readonly string mismatchPasswordValidation = "The new password and confirmation password do not match.";

        private IWebElement Username => Comp.TextInput("UserName");
        private IWebElement Email => Comp.TextInput("Email");
        private IWebElement Password => Comp.TextInput("Password");
        private IWebElement ConfirmPassword => Comp.TextInput("ConfirmPassword");
        public void Open()
        {
            Driver.Navigate().GoToUrl(registerUrl);
            Wait.Until(x => Username.Displayed);
            context.Get<Logger>().Log("Registration page displayed");
        }

        public void SubmitRegistration()
        {
            Comp.Button("Register").Click();
        }

        public void EnterValidRegistrationDetails()
        {
            context.Get<Logger>().Log("Entering valid registration details...");
            Username.ClearAndEnter(NewRegUser.Username);
            Email.ClearAndEnter($"{NewRegUser.Username}@Test.com");
            Password.ClearAndEnter(NewRegUser.Password);
            ConfirmPassword.ClearAndEnter(NewRegUser.Password);
        }

        internal void EnterExistingUserRegistrationDetails()
        {
            context.Get<Logger>().Log("Entering existing user registration details...");
            Username.SendKeys(TestUser.Username);
            Email.SendKeys($"{TestUser.Username}@Test.com");
            Password.SendKeys(NewRegUser.Password);
            ConfirmPassword.SendKeys(NewRegUser.Password);
        }

        public void VerifyTestUserIsCreated()
        {
            context.Get<Logger>().Log("Verifying hte new user is created...");
            context.Get<UsersPage>().VerifyTestUserIsPresent();
        }

        internal void VerifyFieldAndSummaryValidations(string field, string fieldValidation, string summaryValidation)
        {
            ValidationSummary.Displayed.Should().BeTrue();
            ValidationSummary.Text.Replace(Environment.NewLine, string.Empty).Should().Contain(summaryValidation.Trim());
            FieldValidation(field).Text.Should().Contain(fieldValidation);
        }

        internal void VerifyFieldValidations()
        {
            ValidationSummary.Displayed.Should().BeTrue();
            ValidationSummary.Text.Should().Contain(usernameValidation);
            ValidationSummary.Text.Should().Contain(emailValidation);
            ValidationSummary.Text.Should().Contain(passwordValidation);
            FieldValidation("Username").Text.Should().Be(usernameValidation);
            FieldValidation("Email").Text.Should().Be(emailValidation);
            FieldValidation("Password").Text.Should().Be(passwordValidation);
        }

        internal void EnterValidButDifferentPassword()
        {
            Password.ClearAndEnter(NewRegUser.Password);
            ConfirmPassword.ClearAndEnter("MismatchPass123");
        }

        internal void VerifyPasswordMismatchValidation()
        {
            ValidationSummary.Displayed.Should().BeTrue();
            ValidationSummary.Text.Should().Contain(mismatchPasswordValidation);
            FieldValidation("ConfirmPassword").Text.Should().Be(mismatchPasswordValidation);
        }

        public void EnterRegistrationValueInTheField(string value, string field)
        {
            if (field == "Password")
            {
                Password.ClearAndEnter(value);
                ConfirmPassword.ClearAndEnter(value);
            }
            else
            {
                Comp.TextInput(field).ClearAndEnter(value);
            }
        }

        internal void RegisterNewTestApplicant()
        {
            if (!Driver.Url.Contains(registerUrl))
                Open();
            context.Get<Logger>().Log("Registering a new applicant...");
            string timeStamp = DateTime.Now.ToString("ddMMHHmmss");
            Username.ClearAndEnter(NewRegUser.TestApplicantUsername + timeStamp);
            Email.ClearAndEnter($"{NewRegUser.TestApplicantUsername + timeStamp}@Test.com");
            Password.ClearAndEnter(NewRegUser.Password);
            ConfirmPassword.ClearAndEnter(NewRegUser.Password);
            SubmitRegistration();
            context.Add("TestApplicant", NewRegUser.TestApplicantUsername + timeStamp);
        }
    }
}

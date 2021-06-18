using BEIS.GDS.UI.Tests.Support.Common;
using FluentAssertions;
using NetZero.Automated.UI.Tests.TestData;
using NetZero.Automated.UI.Tests.Utils;
using OpenQA.Selenium;
using System;
using TechTalk.SpecFlow;

namespace NetZero.Automated.UI.Tests.Pages
{
    public class LoginPage : BasePage
    {
        private readonly string loginUrl;
        private readonly string invalidLoginValidation = "Invalid login attempt.";
        ScenarioContext context;
        public LoginPage(ScenarioContext context) : base(context)
        {
            this.context = context;
            loginUrl = BaseUrl + "/login";
        }

        private IWebElement Username => Comp.TextInput("UserName");
        private IWebElement Password => Comp.TextInput("Password");
        

        public void Open()
        {
            Driver.Navigate().GoToUrl(loginUrl);
            Wait.Until(x => Username.Displayed);
            VerifyLoginPageIsOpen();
        }

        private void EnterUserLoginDetails(string username, string password)
        {
            Username.ClearAndEnter(username);
            Password.ClearAndEnter(password);
        }

        private void SubmitLogin()
        {
            Comp.Button("Sign in").Click();
        }

        public void LoginUser(string username, string password)
        {
            Open();
            EnterUserLoginDetails(username, password);
            SubmitLogin();
            VerifyPageIsDirectedToHomepage();
        }

        public void LoginWithInvalidDetails()
        {
            context.Get<Logger>().Log("Logging in with invalid credentials...");
            EnterUserLoginDetails(TestUser.Username, TestUser.IncorrectPassword);
            SubmitLogin();
        }

        public void LoginTestUser()
        {
            context.Get<Logger>().Log("Logging in as TestUser...");
            EnterUserLoginDetails(TestUser.Username, TestUser.Password);
            SubmitLogin();
        }

        internal void VerifyIncompleteLoginEntryValidation(string username, string password, string fieldValidation, string validationSummary)
        {
            
            if(string.IsNullOrEmpty(username))
                fieldValidation.Should().Contain(FieldValidation("Username").Text);
            if (string.IsNullOrEmpty(password))
                fieldValidation.Should().Contain(FieldValidation("Password").Text);
            ValidationSummary.Displayed.Should().BeTrue();
            ValidationSummary.Text.Replace(Environment.NewLine, string.Empty).Should().Contain(validationSummary.Trim());
        }

        internal void VerifyApplicationIsAccessible()
        {
            BasicInformationInputPage basicInfoPage = new BasicInformationInputPage(context);
            basicInfoPage.Open();
            basicInfoPage.IsThePageLoaded().Should().BeTrue();
        }

        internal void VerifyLoginPageIsOpen()
        {
            Comp.Button("Sign in").Displayed.Should().BeTrue();
            context.Get<Logger>().Log("Login page displayed");
        }

        internal void VerifyInvalidLoginValidation()
        {
            ValidationSummary.Displayed.Should().BeTrue();
            ValidationSummary.Text.Replace(Environment.NewLine, string.Empty).Should().Contain(invalidLoginValidation.Trim());
        }

        internal void EnterLoginValuesAndSubmit(string username, string password)
        {
            EnterUserLoginDetails(username, password);
            SubmitLogin();
        }

        public void LoginAdminUser()
        {
            Open();
            EnterUserLoginDetails(AdminUser.Username, AdminUser.Password);
            SubmitLogin();
        }

        internal void LoginNewUser()
        {
            if (!Driver.Url.Contains(loginUrl))
                Open();
            EnterLoginValuesAndSubmit(context.Get<string>("TestApplicant"), NewRegUser.Password);

        }

    }

}

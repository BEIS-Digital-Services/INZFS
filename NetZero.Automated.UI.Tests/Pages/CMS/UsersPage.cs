using System.Collections.Generic;
using FluentAssertions;
using NetZero.Automated.UI.Tests.TestData;
using OpenQA.Selenium;
using System.Collections.ObjectModel;
using System.Linq;
using NetZero.Automated.UI.Tests.Utils;
using TechTalk.SpecFlow;
using NetZero.Automated.UI.Tests.Helpers;

namespace NetZero.Automated.UI.Tests.Pages.CMS
{
    public class UsersPage : BasePage
    {
        private string adminUsersUrl;
        private ScenarioContext context;

        private readonly string adminAccessToken;

        public UsersPage(ScenarioContext context) : base(context)
        {
            this.context = context;
            adminUsersUrl = BaseUrl + "/Admin/Users/Index";
            this.adminAccessToken = HttpHelpers.AdminAccessCookie;
        }

        IWebElement AddUserButton => Driver.FindElement(By.LinkText("Add User"));

        ReadOnlyCollection<IWebElement> Users => Driver.FindElements(By.CssSelector("li.list-group-item"));

        IEnumerable<IWebElement> TestUsers => Users.Where(x => x.Text.Contains(NewRegUser.Username+"@Test.com"));
        public void WaitForUsersPageToLoad()
        {
            Wait.Until(x => AddUserButton.Displayed);
        }

        public void Open()
        {
            //Driver.Manage().Cookies.AddCookie(new Cookie("orchauth_Default", adminAccessToken));
            Driver.Navigate().GoToUrl(adminUsersUrl);
            WaitForUsersPageToLoad();
        }
        public bool IsTestUserPresent()
        {
            return TestUsers.Any();
        }

        public void DeleteTestUsers()
        {
            context.Get<LoginPage>().LoginAdminUser();
            Open();            
            DeleteTestUserViaUi();
            Driver.Navigate().Refresh();
            Driver.Manage().Cookies.DeleteCookieNamed("orchauth_Default");            
            IsTestUserPresent().Should().BeFalse();
        }

        private void DeleteTestUserViaUi()
        {
            if (TestUsers.Any())
            {
                TestUsers.FirstOrDefault().FindElement(By.CssSelector("a.btn.btn-danger.btn-sm")).Click();
                Driver.FindElement(By.Id("modalOkButton")).Click();                
                context.Get<Logger>().Log("Test users found and deleted");
            }
        }

        private void DeleteTestUserViaHttp()
        {
            var url = TestUsers.First()
                                     .FindElement(By.CssSelector("a.btn.btn-danger.btn-sm")).GetAttribute("href");
            new HttpHelpers().DeleteTestUser(url);
        }

        public void VerifyTestUserIsPresent()
        {
            context.Get<LoginPage>().LoginAdminUser();
            Open();
            IsTestUserPresent().Should().BeTrue();
            DeleteTestUserViaUi();
        }
        

        
    }
}

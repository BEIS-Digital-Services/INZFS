using NetZero.Automated.UI.Tests.Utils;

namespace NetZero.Automated.UI.Tests.TestData
{
    public static class NewRegUser
    {
        public static string Username => "NewRegTestUser";
        public static string Password => ConfigurationSetUp.TestPassword;

        public static string TestApplicantUsername => "TestApplicant";
    }

    public class AdminUser
    {
        public static string Username => ConfigurationSetUp.AdminUser;
        public static string Password => ConfigurationSetUp.AdminPassword;
    }

    public class TestUser
    {
        public static string Username => ConfigurationSetUp.TestUser;
        public static string Password => ConfigurationSetUp.TestPassword;
        public static string IncorrectPassword => "TestTestTest";
    }

}

namespace INZFS.Theme.Models
{
    public class NotificationOption
    {
        public string EmailVerificationTemplate { get; set; }
        public string SmsCodeTemplate { get; set; }
        public string EmailCodeTemplate { get; set; }
        public string EmailChangePasswordTemplate { get; set; }
        public string ChangeEmailTemplate { get; set; }
        public string ForgotPasswordEmailTemplate { get; set; }
        public string ForgotPasswordConfirmEmailTemplate { get; set; }
        public string AuthenticationChangeTemplate { get; set; }
    }
}
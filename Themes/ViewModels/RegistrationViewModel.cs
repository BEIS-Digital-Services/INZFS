using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using INZFS.Theme.Attributes;

namespace INZFS.Theme.ViewModels
{
    public class MyAccountViewModel
    {
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string ReturnUrl { get; set; }
        public bool IsAuthenticatorEnabled { get; set; }
        public bool IsSmsEnabled { get; set; }
    }

    public class RegistrationViewModel
    {
        [Required]
        [RegularExpression(EmailValidationConstants.EmailValidationExpression, ErrorMessage = EmailValidationConstants.EmailValidationMessage)]
        public string Email { get; set; }

        [Compare("Email")]
        public string ConfirmEmail { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [MustBeTrue(ErrorMessage = "Please consent to BEIS processing your personal information.")]
        public bool IsConsentedToUsePersonalInformation { get; set; }
        
        
        [MustBeTrue(ErrorMessage = "Please consent to BEIS contacting you by email and SMS.")]
        public bool IsConsentedUseEmailAndSms { get; set; }

    }

    public class RegistrationSuccessViewModel
    {
        public string Email { get; set; }
        public bool VerificationRequired { get; set; }
    }

    public class EmailValidationConstants
    {
        public const string EmailValidationExpression = @"^([\w\.\-\+]+)@([\w-]+\.)+[\w-]{2,10}$";
        public const string EmailValidationMessage = "The Email field is not a valid e-mail address.";
    }
}

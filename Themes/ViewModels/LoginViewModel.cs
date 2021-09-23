using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace INZFS.Theme.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email address")]
        [RegularExpression(EmailValidationConstants.EmailValidationExpression, ErrorMessage = EmailValidationConstants.EmailValidationMessage)]
        public string EmailAddress { get; set; }

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
        
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [Display(Name = "Email address")]
        [RegularExpression(EmailValidationConstants.EmailValidationExpression, ErrorMessage = EmailValidationConstants.EmailValidationMessage)]
        public string EmailAddress { get; set; }
        
    }
    
    public class ResetPasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match. Please re-enter your new password.")]
        public string ConfirmPassword { get; set; }

    }
}

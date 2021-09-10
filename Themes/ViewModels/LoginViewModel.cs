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
}

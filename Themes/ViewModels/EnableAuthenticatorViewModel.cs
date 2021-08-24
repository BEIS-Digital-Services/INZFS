using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.Theme.ViewModels
{
    public class ChangeEmailViewModel
    {
        public string CurrentEmail { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Compare("Email")]
        public string ConfirmEmail { get; set; }


        

        
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

    }

    public class ChangePhoneViewModel
    {
        [Required(ErrorMessage = "The Change your security preference field is required.")]
        public ChangeAction? ChosenAction { get; set; }

        public string PhoneNumber { get; set; }
    }

    public class ChangeScanQrForAuthenticatorViewModel
    {
        [Required(ErrorMessage = "The Change your authenticator preference field is required.")]
        public ChangeAction? ChosenAction { get; set; }
    }

    public class ChooseVerificationMethodViewModel
    {
        [Required(ErrorMessage = "The choose verification method field is required.")]
        public AuthenticationMethod? AuthenticationMethod { get; set; }
    }

    public class ChooseAlternativeMethodViewModel
    {
        public ChooseAlternativeMethodViewModel()
        {
            Methods = new List<ChooseAlternativeMethodItem>();
        }

        [Required(ErrorMessage = "The choose verification method field is required.")]
        public AuthenticationMethod? AuthenticationMethod { get; set; }

        public List<ChooseAlternativeMethodItem> Methods { get; set; }
    }

    public class ChooseAlternativeMethodItem
    {
        public AuthenticationMethod Method { get; set; }
        public string Title { get; set; }
    }

    public class AddPhoneNumberViewModel
    {
        [Required(ErrorMessage = "The phone number field is required.")]
        [Phone]
        public string PhoneNumber { get; set; }
    }
    
    public class ChangePhoneNumberViewModel
    {
        [Required(ErrorMessage = "The phone number field is required.")]
        [Phone]
        public string PhoneNumber { get; set; }

        public string CurrentPhoneNumber { get; set; }
    }

    public class EnableAuthenticatorQrCodeViewModel
    {
        public string SharedKey { get; set; }
        public string AuthenticatorUri { get; set; }
        
    }

    public class EnterCodeViewModel
    {
        [Required(ErrorMessage = "The code field is required")]
        public string Code { get; set; }
        public bool IsActivated { get; set; }
        public AuthenticationMethod Method { get; set; }
        public string Message { get; set; }
    }
   
   

    public enum AuthenticationMethod
    {
        None,
        Authenticator,
        Phone,
        Email,
        ChangePhone
    }
    
    public enum ChangeAction
    {
        None,
        Remove,
        Change
    }
}

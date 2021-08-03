using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.Theme.ViewModels
{
    public class ChooseVerificationMethodViewModel
    {
        [Required(ErrorMessage = "The choose verification method field is required.")]
        public AuthenticationMethod? AuthenticationMethod { get; set; }
    }

    public class AddPhoneNumberViewModel
    {
        [Required(ErrorMessage = "The phone number field is required.")]
        public string PhoneNumber { get; set; }
    }

    public class EnableAuthenticatorQrCodeViewModel
    {
        public string SharedKey { get; set; }
        public string AuthenticatorUri { get; set; }
        
    }

    public class EnableAuthenticatorCodeViewModel
    {
        [Required(ErrorMessage = "The authenticator code field is required")]
        public string AuthenticatorCode { get; set; }
        public bool IsActivated { get; set; }

    }

    public enum AuthenticationMethod
    {
        None,
        Authenticator,
        SMS,
        Email
    }
}

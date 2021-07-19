using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.Theme.ViewModels
{
    public class EnableTwoFactorOptionViewModel
    {

    }

    public class EnableAuthenticatorQrCodeViewModel
    {
        public string SharedKey { get; set; }
        public string AuthenticatorUri { get; set; }
    }

    public class EnableAuthenticatorCodeViewModel
    {
        [Required(ErrorMessage = "Authenticator Code is required")]
        public string AuthenticatorCode { get; set; }
    }
}

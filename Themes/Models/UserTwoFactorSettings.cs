using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using INZFS.Theme.ViewModels;

namespace INZFS.Theme.Models
{
    public class UserTwoFactorSettings
    {
        public string UserId { get; set; }
        public bool IsTwoFactorEnabled { get; set; }
        public string AuthenticatorKey { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsPhoneNumberConfirmed { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public AuthenticationMethod TwoFactorActiveMethod { get; set; }
    }
}

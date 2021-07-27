using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.Theme.Models
{
    public class UserTwoFactorSettings
    {
        public string UserId { get; set; }
        public bool IsTwoFactorEnabled { get; set; }
        public string AuthenticatorKey { get; set; }
        public string PhoneNumber { get; set; }
        public string IsPhoneNumberConfirmed { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

    }
}

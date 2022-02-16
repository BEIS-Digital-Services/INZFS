using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace INZFS.Theme.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class TwoFactorAuthorizeAttribute : Attribute, IAuthorizeData
    {
        public string? Policy { get; set; }
        public string? Roles { get; set; }
        public string? AuthenticationSchemes { get; set; }

        public TwoFactorAuthorizeAttribute()
        {
            AuthenticationSchemes = IdentityConstants.TwoFactorUserIdScheme;
        }
    }
}

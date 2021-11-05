using System;
using INZFS.Theme.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace INZFS.Theme.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class MyAccountAuthorizeAttribute : Attribute, IAuthorizeData
    {
        public string? Policy { get; set; }
        public string? Roles { get; set; }
        public string? AuthenticationSchemes { get; set; }

        public MyAccountAuthorizeAttribute()
        {
            AuthenticationSchemes = RegistrationConstants.MyAccountScheme;
        }

    }
}
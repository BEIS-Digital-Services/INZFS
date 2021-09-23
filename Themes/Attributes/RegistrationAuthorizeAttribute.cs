using System;
using INZFS.Theme.Services;
using Microsoft.AspNetCore.Authorization;

namespace INZFS.Theme.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RegistrationAuthorizeAttribute : Attribute, IAuthorizeData
    {
        public string? Policy { get; set; }
        public string? Roles { get; set; }
        public string? AuthenticationSchemes { get; set; }

        public RegistrationAuthorizeAttribute()
        {
            AuthenticationSchemes = RegistrationConstants.RegistrationScheme;
        }
    }
}
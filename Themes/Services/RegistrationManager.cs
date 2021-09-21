using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Users;
using OrchardCore.Users.Models;

namespace INZFS.Theme.Services
{
    public interface IRegistrationManager
    {
        Task<SignInResult> RegisterSignInAsync(IUser user);
        Task<IUser> GetRegistrationAuthenticationUserAsync();
    }

    public class RegistrationManager : IRegistrationManager
    {
        protected virtual CancellationToken CancellationToken => CancellationToken.None;

        private readonly IUserStore<IUser> _store;
        private readonly IHttpContextAccessor _httpContext;

        public RegistrationManager(IUserStore<IUser> store,IHttpContextAccessor httpContext)
        {
            _store = store;
            _httpContext = httpContext;
        }
        public async Task<SignInResult> RegisterSignInAsync(IUser user)
        {
            if (user is User nUser)
            { var identity = new ClaimsIdentity(RegistrationConstants.RegistrationScheme);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, nUser.UserId));
                var claimsPrincipal = new ClaimsPrincipal(identity);
                
                await _httpContext.HttpContext.SignInAsync(RegistrationConstants.RegistrationScheme, claimsPrincipal);
                
                return SignInResult.Success;
            }
            return SignInResult.Failed;
        }

        public virtual async Task<IUser> GetRegistrationAuthenticationUserAsync()
        {
            var info = await RetrieveRegistrationInfoAsync();
            if (info == null)
            {
                return null;
            }
            return await _store.FindByIdAsync(info.UserId, CancellationToken);
        }


        private async Task<RegistrationAuthenticationInfo> RetrieveRegistrationInfoAsync()
        {
            var result = await _httpContext.HttpContext.AuthenticateAsync(RegistrationConstants.RegistrationScheme);
            if (result?.Principal != null)
            {
                return new RegistrationAuthenticationInfo
                {
                    UserId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier),
                    LoginProvider = result.Principal.FindFirstValue(ClaimTypes.AuthenticationMethod)
                };
            }
            return null;
        }

    }

    internal class RegistrationAuthenticationInfo
    {
        public string UserId { get; set; }
        public string LoginProvider { get; set; }
    }

    public static class RegistrationConstants
    {
        public const string RegistrationScheme = "Registration";
        public const string RegistrationCookie = "registration";
    }
}

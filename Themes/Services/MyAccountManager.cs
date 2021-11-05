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

    public interface IMyAccountManager
    {
        Task<SignInResult> MyAccountSignInAsync(IUser user);
        Task<IUser> GetMyAccountAuthenticationUserAsync();
        Task<bool> SignOutAsync();
    }

    public class MyAccountManager : IMyAccountManager
    {
        protected virtual CancellationToken CancellationToken => CancellationToken.None;

        private readonly IUserStore<IUser> _store;
        private readonly IHttpContextAccessor _httpContext;

        public MyAccountManager(IUserStore<IUser> store, IHttpContextAccessor httpContext)
        {
            _store = store;
            _httpContext = httpContext;
        }

        public async Task<SignInResult> MyAccountSignInAsync(IUser user)
        {
            if (user is User nUser)
            {
                var identity = new ClaimsIdentity(RegistrationConstants.MyAccountScheme);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, nUser.UserId));
                var claimsPrincipal = new ClaimsPrincipal(identity);

                await _httpContext.HttpContext.SignInAsync(RegistrationConstants.MyAccountScheme, claimsPrincipal);

                return SignInResult.Success;
            }
            return SignInResult.Failed;
        }

        public Task<IUser> GetMyAccountAuthenticationUserAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SignOutAsync()
        {
            await _httpContext.HttpContext.SignOutAsync(RegistrationConstants.MyAccountScheme);
            return true;
        }
    }
}

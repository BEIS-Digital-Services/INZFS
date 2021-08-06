using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using INZFS.Theme.Models;
using INZFS.Theme.Records;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Security.Services;
using OrchardCore.Users;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using YesSql;

namespace INZFS.Theme.Services
{
    public class UserTwoFactorStore : UserStore,
         IUserTwoFactorStore<IUser>,
         IUserPhoneNumberStore<IUser>,
         IUserAuthenticatorKeyStore<IUser>,
         IUserLockoutStore<IUser>
    {
        private readonly IUserTwoFactorSettingsService _twofactorSettingsService;
        private readonly TwoFactorOption _options;

        public UserTwoFactorStore(
            ISession session,
            IRoleService roleService,
            ILookupNormalizer keyNormalizer,
            IUserIdGenerator userIdGenerator,
            ILogger<UserStore> logger,
            IEnumerable<IUserEventHandler> handlers,
            IDataProtectionProvider dataProtectionProvider,
            IUserTwoFactorSettingsService twofactorSettingsService,
            IOptions<TwoFactorOption> options)
                : base(session, roleService, keyNormalizer, userIdGenerator, logger, handlers, dataProtectionProvider)
        {
            _twofactorSettingsService = twofactorSettingsService;
            _options = options?.Value;
        }

        public async Task SetTwoFactorEnabledAsync(IUser user, bool enabled, CancellationToken cancellationToken)
        {
            var userId = GetUserId(user);
            await _twofactorSettingsService.SetTwoFactorEnabledAsync(userId, enabled);
        }

        public async Task<bool> GetTwoFactorEnabledAsync(IUser user, CancellationToken cancellationToken)
        {
            if (_options.Status == TwoFactorStatus.Disabled)
            {
                return false;
            }

            if (_options.Status == TwoFactorStatus.Optional)
            {
                var userId = GetUserId(user);
                return await _twofactorSettingsService.GetTwoFactorEnabledAsync(userId);
            }

            return true; 
        }

        public async Task SetPhoneNumberAsync(IUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            var userId = GetUserId(user);
            await _twofactorSettingsService.SetPhoneNumberAsync(userId, phoneNumber);
        }

        public async Task<string> GetPhoneNumberAsync(IUser user, CancellationToken cancellationToken)
        {
            var userId = GetUserId(user);
            return await _twofactorSettingsService.GetPhoneNumberAsync(userId);
        }

        public async Task<bool> GetPhoneNumberConfirmedAsync(IUser user, CancellationToken cancellationToken)
        {
            var userId = GetUserId(user);
            return await _twofactorSettingsService.GetPhoneNumberConfirmedAsync(userId);
        }

        public async Task SetPhoneNumberConfirmedAsync(IUser user, bool confirmed, CancellationToken cancellationToken)
        {
            var userId = GetUserId(user);
            await _twofactorSettingsService.SetPhoneNumberConfirmedAsync(userId, confirmed);
        }

        public async Task SetAuthenticatorKeyAsync(IUser user, string key, CancellationToken cancellationToken)
        {
            var userId = GetUserId(user);
            await _twofactorSettingsService.SetAuthenticatorKeyAsync(userId, key);
        }

        public async Task<string> GetAuthenticatorKeyAsync(IUser user, CancellationToken cancellationToken)
        {
            var userId = GetUserId(user);
            return await _twofactorSettingsService.GetAuthenticatorKeyAsync(userId); ;
        }

       private string GetUserId(IUser user)
       {
           return (user as User)?.UserId;
       }

       public async Task<DateTimeOffset?> GetLockoutEndDateAsync(IUser user, CancellationToken cancellationToken)
       {
           return new DateTimeOffset(DateTime.Now.AddDays(-1));
       }

       public Task SetLockoutEndDateAsync(IUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
       {
          return Task.CompletedTask;
       }

       public Task<int> IncrementAccessFailedCountAsync(IUser user, CancellationToken cancellationToken)
       {
           return Task.FromResult(0);
        }

       public Task ResetAccessFailedCountAsync(IUser user, CancellationToken cancellationToken)
       {
           return Task.CompletedTask;
       }

       public Task<int> GetAccessFailedCountAsync(IUser user, CancellationToken cancellationToken)
       {
           return Task.FromResult(0);
        }

       public Task<bool> GetLockoutEnabledAsync(IUser user, CancellationToken cancellationToken)
       {
           return Task.FromResult(false);
       }

       public Task SetLockoutEnabledAsync(IUser user, bool enabled, CancellationToken cancellationToken)
       {
           return Task.CompletedTask;
       }
    }

}

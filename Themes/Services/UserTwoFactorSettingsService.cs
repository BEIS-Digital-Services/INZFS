﻿using System.Threading.Tasks;
using INZFS.Theme.Models;
using INZFS.Theme.Records;
using YesSql;

namespace INZFS.Theme.Services
{
    public class UserTwoFactorSettingsService : IUserTwoFactorSettingsService
    {
        private readonly ISession _session;

        public UserTwoFactorSettingsService(ISession session)
        {
            _session = session;
        }

        public async Task<string> GetAuthenticatorKeyAsync(string userId)
        {
            var userTwoFactorSettings = await GetUserTwoFactorSettings(userId);
            return userTwoFactorSettings?.AuthenticatorKey;
        }

        public async Task<bool> SetAuthenticatorKeyAsync(string userId, string key)
        {
            var userTwoFactorSettings = await GetUserTwoFactorSettings(userId);
            if (userTwoFactorSettings == null)
            {
                _session.Save(new UserTwoFactorSettings
                {
                    UserId = userId, 
                    IsTwoFactorEnabled = false, 
                    AuthenticatorKey = key
                });
            }
            else
            {
                userTwoFactorSettings.AuthenticatorKey = key;
                _session.Save(userTwoFactorSettings);
            }

            //TODO: FM need to check how save works for YesSql with multi-threaded env
            //await _session.CommitAsync();

            return true;
        }

        public async Task<bool> SetTwoFactorEnabledAsync(string userId, bool enabled)
        {
            var userTwoFactorSettings = await GetUserTwoFactorSettings(userId);
       
            userTwoFactorSettings.IsTwoFactorEnabled = true;
            _session.Save(userTwoFactorSettings);

            //TODO: FM need to check how save works for YesSql with multi-threaded env
            //await _session.CommitAsync();

            return true;
        }

        public async Task<bool> GetTwoFactorEnabledAsync(string userId)
        {
            var userTwoFactorSettings = await GetUserTwoFactorSettings(userId);
            return userTwoFactorSettings?.IsTwoFactorEnabled ?? false;
        }

        private async Task<UserTwoFactorSettings> GetUserTwoFactorSettings(string userId)
        {
            var query = _session.Query<UserTwoFactorSettings, UserTwoFactorSettingsIndex>();
            query = query.With<UserTwoFactorSettingsIndex>(index => index.UserId == userId);
            var userTwoFactorSettings = await query.FirstOrDefaultAsync();
            return userTwoFactorSettings;
        }
    }
}
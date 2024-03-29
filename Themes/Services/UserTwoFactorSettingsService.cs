﻿using System;
using System.Threading.Tasks;
using INZFS.Theme.Models;
using INZFS.Theme.Records;
using INZFS.Theme.ViewModels;
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
            userTwoFactorSettings.AuthenticatorKey = key;
            await SaveSettings(userTwoFactorSettings);
            return true;
        }

        public async Task<bool> SetTwoFactorEnabledAsync(string userId, bool enabled)
        {
            var userTwoFactorSettings = await GetUserTwoFactorSettings(userId);
            userTwoFactorSettings.IsTwoFactorEnabled = true;
            await SaveSettings(userTwoFactorSettings);
            return true;
        }

        public async Task<bool> GetTwoFactorEnabledAsync(string userId)
        {
            var userTwoFactorSettings = await GetUserTwoFactorSettings(userId);
            return userTwoFactorSettings?.IsTwoFactorEnabled ?? false;
        }

        public async Task<bool> SetPhoneNumberAsync(string userId, string phoneNumber)
        {
            var userTwoFactorSettings = await GetUserTwoFactorSettings(userId);
            userTwoFactorSettings.PhoneNumber = phoneNumber;
            await  SaveSettings(userTwoFactorSettings);
            return true;
        }

        public async Task<string> GetPhoneNumberAsync(string userId)
        {
            var userTwoFactorSettings = await GetUserTwoFactorSettings(userId);
            return userTwoFactorSettings?.PhoneNumber;
        }

        public async Task<bool> GetPhoneNumberConfirmedAsync(string userId)
        {
            var userTwoFactorSettings = await GetUserTwoFactorSettings(userId);
            return userTwoFactorSettings?.IsPhoneNumberConfirmed ?? false;
        }

        public async Task<bool> SetPhoneNumberConfirmedAsync(string userId, bool confirmed)
        {
            var userTwoFactorSettings = await GetUserTwoFactorSettings(userId);
            userTwoFactorSettings.IsPhoneNumberConfirmed = confirmed;
            userTwoFactorSettings.TwoFactorActiveMethod = AuthenticationMethod.Phone;

            await SaveSettings(userTwoFactorSettings);
            return true;
        }

        public async Task<bool> SetAuthenticatorConfirmedAsync(string userId, bool confirmed)
        {
            var userTwoFactorSettings = await GetUserTwoFactorSettings(userId);
            userTwoFactorSettings.IsAuthenticatorConfirmed = confirmed;
            userTwoFactorSettings.TwoFactorActiveMethod = AuthenticationMethod.Authenticator;

            await SaveSettings(userTwoFactorSettings);
            return true;
        }

        public async Task<bool> GetAuthenticatorConfirmedAsync(string userId)
        {
            var userTwoFactorSettings = await GetUserTwoFactorSettings(userId);
            return userTwoFactorSettings?.IsAuthenticatorConfirmed ?? false;
        }

        public async Task<AuthenticationMethod> GetTwoFactorDefaultAsync(string userId)
        {
            var userTwoFactorSettings = await GetUserTwoFactorSettings(userId);
            return userTwoFactorSettings?.TwoFactorActiveMethod ?? AuthenticationMethod.None;
        }

        public async Task<bool> SetTwoFactorDefaultAsync(string userId, AuthenticationMethod method)
        {
            var userTwoFactorSettings = await GetUserTwoFactorSettings(userId);
            userTwoFactorSettings.TwoFactorActiveMethod = method;
            await SaveSettings(userTwoFactorSettings);
            return true;
        }

        private async Task SaveSettings(UserTwoFactorSettings userTwoFactorSettings)
        {
            _session.Save(userTwoFactorSettings);
            await _session.SaveChangesAsync();

        }

        private async Task<UserTwoFactorSettings> GetUserTwoFactorSettings(string userId)
        {
            var query = _session.Query<UserTwoFactorSettings, UserTwoFactorSettingsIndex>();
            query = query.With<UserTwoFactorSettingsIndex>(index => index.UserId == userId);
            var userTwoFactorSettings = await query.FirstOrDefaultAsync();

            if (userTwoFactorSettings == null)
            {
                var twoFactorSettings = new UserTwoFactorSettings
                {
                    UserId = userId,
                    IsTwoFactorEnabled = false,
                    CreatedDate = DateTime.Now
                };

                _session.Save(twoFactorSettings);
                await _session.SaveChangesAsync();

                query = _session.Query<UserTwoFactorSettings, UserTwoFactorSettingsIndex>();
                query = query.With<UserTwoFactorSettingsIndex>(index => index.UserId == userId);
                userTwoFactorSettings = await query.FirstOrDefaultAsync();
            }

            return userTwoFactorSettings;
        }
    }
}
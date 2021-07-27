using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.Theme.Services
{
    public interface IUserTwoFactorSettingsService
    {
        Task<string> GetAuthenticatorKeyAsync(string userId);
        Task<bool> SetAuthenticatorKeyAsync(string userId, string key);
        Task<bool> SetTwoFactorEnabledAsync(string userId, bool enabled);
        Task<bool> GetTwoFactorEnabledAsync(string userId);
    }
}

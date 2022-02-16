using System.Threading.Tasks;
using INZFS.Theme.ViewModels;

namespace INZFS.Theme.Services
{
    public interface IUserTwoFactorSettingsService
    {
        Task<string> GetAuthenticatorKeyAsync(string userId);
        Task<bool> SetAuthenticatorKeyAsync(string userId, string key);
        Task<bool> SetTwoFactorEnabledAsync(string userId, bool enabled);
        Task<bool> GetTwoFactorEnabledAsync(string userId);
        Task<bool> SetPhoneNumberAsync(string userId, string phoneNumber);
        Task<string> GetPhoneNumberAsync(string userId);
        Task<bool> GetPhoneNumberConfirmedAsync(string userId);
        Task<bool> SetPhoneNumberConfirmedAsync(string userId, bool confirmed);
        Task<bool> SetAuthenticatorConfirmedAsync(string userId, bool confirmed);
        Task<bool> GetAuthenticatorConfirmedAsync(string userId);
        Task<AuthenticationMethod> GetTwoFactorDefaultAsync(string userId);
        Task<bool> SetTwoFactorDefaultAsync(string userId, AuthenticationMethod method);
    }
}

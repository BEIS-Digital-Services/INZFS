using System.Threading.Tasks;
using INZFS.Theme.ViewModels;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Users;

namespace INZFS.Theme.Services
{
    public interface ITwoFactorAuthenticationService
    {
        Task<EnableAuthenticatorQrCodeViewModel> GetSharedKeyAndQrCodeUriAsync(IUser user);
        Task<IdentityResult> ResetAuthenticatorKeyAsync(IUser user);
    }
}
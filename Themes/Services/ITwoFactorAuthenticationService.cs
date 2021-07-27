using System.Threading.Tasks;
using INZFS.Theme.ViewModels;
using OrchardCore.Users;

namespace INZFS.Theme.Services
{
    public interface ITwoFactorAuthenticationService
    {
        Task<EnableAuthenticatorQrCodeViewModel> GetSharedKeyAndQrCodeUriAsync(IUser user);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using INZFS.Theme.ViewModels;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Users;

namespace INZFS.Theme.Services
{
    public class TwoFactorAuthenticationService : ITwoFactorAuthenticationService
    {
        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
        
        private readonly UserManager<IUser> _userManager;
        private readonly UrlEncoder _urlEncoder;

        public TwoFactorAuthenticationService(UserManager<IUser> userManager, UrlEncoder urlEncoder)
        {
            _userManager = userManager;
            _urlEncoder = urlEncoder;
        }

        public async Task<EnableAuthenticatorQrCodeViewModel> GetSharedKeyAndQrCodeUriAsync(IUser user)
        {
            var model = new EnableAuthenticatorQrCodeViewModel();

            var key = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(key))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                key = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            model.SharedKey = GetFormattedKey(key);

            var email = await _userManager.GetEmailAsync(user);
            model.AuthenticatorUri = GetGeneratedQrCodeUri(email, key);

            return model;
        }

        private string GetFormattedKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GetGeneratedQrCodeUri(string email, string unformattedKey)
        {
            //TODO: FM AccountName from config
            return string.Format(
                AuthenticatorUriFormat,
                _urlEncoder.Encode("INZFS"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }
    }
}

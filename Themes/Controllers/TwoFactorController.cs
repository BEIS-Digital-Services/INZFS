using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using INZFS.Theme.Services;
using INZFS.Theme.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.Users;

namespace INZFS.Theme.Controllers
{
    [Authorize]
    public class TwoFactorController : Controller
    {
        private readonly UserManager<IUser> _userManager;
        private readonly ITwoFactorAuthenticationService _twoFactorAuthenticationService;
        private readonly ILogger<TwoFactorController> _logger;

        public TwoFactorController(
            UserManager<IUser> userManager,
            ITwoFactorAuthenticationService twoFactorAuthenticationService,
            ILogger<TwoFactorController> logger)
        {
            _userManager = userManager;
            _twoFactorAuthenticationService = twoFactorAuthenticationService;
            _logger = logger;
        }


        public async Task<IActionResult> Enable()
        {
            return View(new EnableTwoFactorOptionViewModel());
        }

        public async Task<IActionResult> ScanQr()
        {
            var user = await _userManager.GetUserAsync(User);
            var model = await _twoFactorAuthenticationService.GetSharedKeyAndQrCodeUriAsync(user);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AuthenticatorCode()
        {
            var model = new EnableAuthenticatorCodeViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AuthenticatorCode(EnableAuthenticatorCodeViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (ModelState.IsValid)
            {
                var authenticatorCode = model.AuthenticatorCode
                    .Replace(" ", string.Empty)
                    .Replace("-", string.Empty);

                var isValidToken = await _userManager.VerifyTwoFactorTokenAsync(
                    user, _userManager.Options.Tokens.AuthenticatorTokenProvider, authenticatorCode);

                if (isValidToken)
                {
                    await _userManager.SetTwoFactorEnabledAsync(user, true);
                    var userId = await _userManager.GetUserIdAsync(user);
                    _logger.LogInformation("User '{UserId}' has successfully enabled 2FA with an authenticator app.", userId);

                    //TODO: should redirect to redirect url or authentication landing page
                    return LocalRedirect("/");
                }

                ModelState.AddModelError("AuthenticatorCode", "Verification code is not valid, please enter a valid code and try again");
            }

            return View(model);
        }
    }
}

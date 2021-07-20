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
    [Authorize(AuthenticationSchemes = "Identity.TwoFactorUserId")]
    public class TwoFactorController : Controller
    {
        private readonly UserManager<IUser> _userManager;
        private readonly ITwoFactorAuthenticationService _twoFactorAuthenticationService;
        private readonly IUserTwoFactorSettingsService _factorSettingsService;
        private readonly SignInManager<IUser> _signInManager;
        private readonly ILogger<TwoFactorController> _logger;

        public TwoFactorController(
            UserManager<IUser> userManager,
            ITwoFactorAuthenticationService twoFactorAuthenticationService,
            IUserTwoFactorSettingsService factorSettingsService,
            SignInManager<IUser> signInManager,
            ILogger<TwoFactorController> logger)
        {
            _userManager = userManager;
            _twoFactorAuthenticationService = twoFactorAuthenticationService;
            _factorSettingsService = factorSettingsService;
            _signInManager = signInManager;
            _logger = logger;
        }
        
        public async Task<IActionResult> Select(string returnUrl)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            var model = new EnableTwoFactorOptionViewModel();

            var userId = await _userManager.GetUserIdAsync(user);

            
            if (await _factorSettingsService.GetTwoFactorEnabledAsync(userId))
            {
                model.LoginAction = "AuthenticatorCode";
            }
            else
            {
                model.LoginAction = "ScanQr";
            }
            
            return View(model);
        }

        public async Task<IActionResult> ScanQr(string returnUrl)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            var model = await _twoFactorAuthenticationService.GetSharedKeyAndQrCodeUriAsync(user);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AuthenticatorCode(string returnUrl)
        {
            var model = new EnableAuthenticatorCodeViewModel( );
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AuthenticatorCode(EnableAuthenticatorCodeViewModel model, string returnUrl)
        {
            returnUrl ??= Url.Content("~/");

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
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
                    var userId = await _userManager.GetUserIdAsync(user);
                    if (!await _factorSettingsService.GetTwoFactorEnabledAsync(userId))
                    {
                        await _userManager.SetTwoFactorEnabledAsync(user, true);
                    }
                    
                    var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, false, false);
                    if (result.Succeeded)
                    {
                        //_logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.UserName);
                        return LocalRedirect(returnUrl);
                    }
                }

                ModelState.AddModelError("AuthenticatorCode", "Verification code is not valid, please enter a valid code and try again");
            }

            return View(model);
        }
        
    }
}

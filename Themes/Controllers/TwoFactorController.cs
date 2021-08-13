using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using INZFS.Theme.Models;
using INZFS.Theme.Services;
using INZFS.Theme.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly INotificationService _notificationService;
        private readonly IUrlEncodingService _encodingService;
        private readonly NotificationOption _notificationOption;

        public TwoFactorController(
            UserManager<IUser> userManager,
            ITwoFactorAuthenticationService twoFactorAuthenticationService,
            IUserTwoFactorSettingsService factorSettingsService,
            SignInManager<IUser> signInManager,
            ILogger<TwoFactorController> logger, 
            INotificationService notificationService,
            IUrlEncodingService encodingService, 
            IOptions<NotificationOption> notificationOption)
        {
            _userManager = userManager;
            _twoFactorAuthenticationService = twoFactorAuthenticationService;
            _factorSettingsService = factorSettingsService;
            _signInManager = signInManager;
            _logger = logger;
            _notificationService = notificationService;
            _encodingService = encodingService;
            _notificationOption = notificationOption.Value;
        }

        [HttpGet]
        public async Task<IActionResult> Select(string returnUrl)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (await IsTwoFactorActivated(user))
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var method = await _factorSettingsService.GetTwoFactorDefaultAsync(userId);
                if (method == AuthenticationMethod.Phone)
                {
                    await SendSms(user);
                }
                
                return RedirectToAction("EnterCode",  new { method,  returnUrl });
            }

            var model = new ChooseVerificationMethodViewModel();
            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> Select(ChooseVerificationMethodViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (model.AuthenticationMethod == AuthenticationMethod.Authenticator)
                {
                    return RedirectToAction("ScanQr", new {returnUrl});
                }
                
                if (model.AuthenticationMethod == AuthenticationMethod.Phone)
                {
                    return RedirectToAction("AddPhoneNumber", new { returnUrl });
                }

                ModelState.AddModelError(nameof(model.AuthenticationMethod), "Invalid selection");
            }

            return View(model);
        }

        
        public async Task<IActionResult> ScanQr(string returnUrl)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (await IsTwoFactorActivated(user))
            {
                return RedirectToAction("EnterCode", new { method = AuthenticationMethod.None, returnUrl });
            }

            var model = await _twoFactorAuthenticationService.GetSharedKeyAndQrCodeUriAsync(user);

            return View(model);
        }  
        
        [HttpGet]
        public async Task<IActionResult> AddPhoneNumber(string returnUrl)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (await IsTwoFactorActivated(user))
            {
                return RedirectToAction("EnterCode", new { method = AuthenticationMethod.None, returnUrl });
            }

            var model = new AddPhoneNumberViewModel();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPhoneNumber(AddPhoneNumberViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
                await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                await SendSms(user);
                return RedirectToAction("EnterCode", new { method = AuthenticationMethod.Phone, returnUrl });
            }

            return View(model);
        }

        private async Task SendSms(IUser user)
        {
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, AuthenticationMethod.Phone.ToString());
            var parameters = new Dictionary<string, dynamic>();
            parameters.Add("code", code);
            await _notificationService.SendSmsAsync(phoneNumber, _notificationOption.SmsCodeTemplate, parameters);
        }
        
        private async Task SendEmail(IUser user)
        {
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, AuthenticationMethod.Email.ToString());
            var parameters = new Dictionary<string, dynamic>();
            parameters.Add("code", code);
            await _notificationService.SendEmailAsync(email, _notificationOption.EmailCodeTemplate, parameters);
        }

        [HttpGet]
        public async Task<IActionResult> EnterCode(AuthenticationMethod method, string returnUrl, string token)
        {
            var model = new EnterCodeViewModel();
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            model.IsActivated = await IsTwoFactorActivated(user);
            model.Method = AuthenticationMethod.Phone;

            if (method == AuthenticationMethod.Phone)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var phone = await _factorSettingsService.GetPhoneNumberAsync(userId);
                if (model.IsActivated)
                {
                    phone = "*******" + phone.Substring(phone.Length - 3);
                }
                model.Message = phone;
            } 
            
            if (method == AuthenticationMethod.Email)
            {
                var email = await _userManager.GetEmailAsync(user);
                if (model.IsActivated)
                {
                    email = _encodingService.MaskEmail(email);
                }
                model.Message = email;
            }

            return View($"{method}Code", model);
        }
        
       

        [HttpPost]
        public async Task<IActionResult> EnterCode(EnterCodeViewModel model, string returnUrl)
        {
            returnUrl ??= Url.Content("~/");

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (ModelState.IsValid)
            {
                var code = model.Code
                    .Replace(" ", string.Empty)
                    .Replace("-", string.Empty);

                var isValidToken = await _userManager.VerifyTwoFactorTokenAsync(user, model.Method.ToString(), code);
                
                if (isValidToken)
                {   
                    var result = await _signInManager.TwoFactorSignInAsync(model.Method.ToString(), code, false, false);
                    if (result?.Succeeded ?? false)
                    {
                        if (!await IsTwoFactorActivated(user))
                        {
                            await SetTwoFactorEnabledAsync(user, true, model.Method);
                        }
                        else
                        {
                            await SetTwoFactorDefaultIfChangedAsync(user, model.Method);
                        }

                        _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.UserName);
                        return LocalRedirect(returnUrl);
                    }
                }

                ModelState.AddModelError("Code", "Verification code is not valid, please enter a valid code and try again");
            }

            return View($"{model.Method}Code", model);
        }

        [HttpGet]
        public async Task<IActionResult> Alternative(string returnUrl)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            var model = new ChooseVerificationMethodViewModel();
           return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> Alternative(ChooseVerificationMethodViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
                await SendEmail(user);
                return RedirectToAction("EnterCode", new { method = AuthenticationMethod.Email, returnUrl });
            }

            return View(model);
        }


        private async Task<bool> IsTwoFactorActivated(IUser user)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            var isActivated = await _factorSettingsService.GetTwoFactorEnabledAsync(userId);
            return isActivated;
        }

        private async Task SetTwoFactorEnabledAsync(IUser user, bool enabled, AuthenticationMethod method)
        {
            await _userManager.SetTwoFactorEnabledAsync(user, enabled);
            var userId = await _userManager.GetUserIdAsync(user);
            if (method == AuthenticationMethod.Phone)
            {
                await _factorSettingsService.SetPhoneNumberConfirmedAsync(userId, enabled);
            }
            else if (method == AuthenticationMethod.Authenticator)
            {
                await _factorSettingsService.SetAuthenticatorConfirmedAsync(userId, enabled);
            }
            else
            {
                await _factorSettingsService.SetTwoFactorDefaultAsync(userId, method);
            }
        } 
        
        private async Task SetTwoFactorDefaultIfChangedAsync(IUser user, AuthenticationMethod method)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            var currentMethod = await _factorSettingsService.GetTwoFactorDefaultAsync(userId);
            if (method != currentMethod)
            {
                await _factorSettingsService.SetTwoFactorDefaultAsync(userId, method);
            }
        }
    }
}

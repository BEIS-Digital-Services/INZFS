using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.Core.Logging;
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
    [Authorize]
    public class MyAccountController : Controller
    {
        private readonly UserManager<IUser> _userManager;
        private readonly SignInManager<IUser> _signInManager;
        private readonly ITwoFactorAuthenticationService _twoFactorAuthenticationService;
        private readonly IUserTwoFactorSettingsService _factorSettingsService;
        private readonly INotificationService _notificationService;
        private readonly IUrlEncodingService _encodingService;
        private readonly ILogger<MyAccountController> _logger;
        private readonly NotificationOption _notificationOption;

        public MyAccountController(UserManager<IUser> userManager,
            SignInManager<IUser> signInManager,
            ITwoFactorAuthenticationService twoFactorAuthenticationService,
            IUserTwoFactorSettingsService factorSettingsService, 
            INotificationService notificationService,
            IUrlEncodingService encodingService,
            IOptions<NotificationOption> notificationOption,
            ILogger<MyAccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _twoFactorAuthenticationService = twoFactorAuthenticationService;
            _factorSettingsService = factorSettingsService;
            _notificationService = notificationService;
            _encodingService = encodingService;
            _logger = logger;
            _notificationOption = notificationOption.Value;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string returnUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = await _userManager.GetUserIdAsync(user);

            var model = new MyAccountViewModel();
            model.ReturnUrl = Url.Action("Index");
            model.EmailAddress = await _userManager.GetEmailAsync(user);
            model.PhoneNumber = await _userManager.GetPhoneNumberAsync(user);

            model.IsAuthenticatorEnabled = await _factorSettingsService.GetAuthenticatorConfirmedAsync(userId);
            model.IsSmsEnabled = await _factorSettingsService.GetPhoneNumberConfirmedAsync(userId);

            return View(model);
        }



        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        } 
        
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }

                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (changePasswordResult.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    _logger.LogInformation("User changed their password successfully.");
                    var email = await _userManager.GetEmailAsync(user);
                    await _notificationService.SendEmailAsync(email, _notificationOption.EmailChangePasswordTemplate);
                    return RedirectToAction("SuccessPasswordChange");
                }

                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        } 
        
        public IActionResult SuccessPasswordChange()
        {
          return View();
        }

        public async Task<IActionResult> AddScanQr()
        {
            var user = await _userManager.GetUserAsync(User);
            var model = await _twoFactorAuthenticationService.GetSharedKeyAndQrCodeUriAsync(user);
            return View("ScanQr", model);
        }

        [HttpGet]
        public async Task<IActionResult> ChangeScanQr()
        {
            return View(new ChangeScanQrForAuthenticatorViewModel());
        }

        
        [HttpPost]
        public async Task<IActionResult> ChangeScanQr(ChangeScanQrForAuthenticatorViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var userId = await _userManager.GetUserIdAsync(user);
                var result = await _twoFactorAuthenticationService.ResetAuthenticatorKeyAsync(user);
                if (result.Succeeded)
                {
                    var defaultMethod = await _factorSettingsService.GetTwoFactorDefaultAsync(userId);
                    if (defaultMethod == AuthenticationMethod.Authenticator)
                    {
                        await _factorSettingsService.SetTwoFactorDefaultAsync(userId, AuthenticationMethod.Email);
                        await _factorSettingsService.SetAuthenticatorConfirmedAsync(userId, false);
                    }

                    if (model.ChosenAction == ChangeAction.Change)
                    {
                        return RedirectToAction("AddScanQr");
                    }

                    if (model.ChosenAction == ChangeAction.Remove)
                    {
                        return RedirectToAction("Index");
                    }

                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AddPhoneNumber()
        {
            var model = new AddPhoneNumberViewModel();
            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> AddPhoneNumber(AddPhoneNumberViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
                var code = await _userManager.GenerateTwoFactorTokenAsync(user, AuthenticationMethod.Phone.ToString());
                await SendSms(phoneNumber, code);
                return RedirectToAction("EnterCode", new { method = AuthenticationMethod.Phone });
            }

            return View(model);
        }

        

        [HttpGet]
        public async Task<IActionResult> ChangePhone()
        {
            return View(new ChangePhoneViewModel());
        }


        [HttpPost]
        public async Task<IActionResult> ChangePhone(ChangePhoneViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.ChosenAction == ChangeAction.Change)
                {
                    return RedirectToAction("ChangePhoneNumber");
                }

                if (model.ChosenAction == ChangeAction.Remove)
                {
                    var userId = await GetUserId();
                    await _factorSettingsService.SetPhoneNumberConfirmedAsync(userId, false);
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        

        [HttpGet]
        public async Task<IActionResult> ChangePhoneNumber()
        {
            var model = new ChangePhoneNumberViewModel();
            model.CurrentPhoneNumber = await _factorSettingsService.GetPhoneNumberAsync(await GetUserId());
            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> ChangePhoneNumber(ChangePhoneNumberViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);
                await SendSms(model.PhoneNumber, code);
                return RedirectToAction("EnterCode", new { method = AuthenticationMethod.ChangePhone, token = _encodingService.GetHexFromString(model.PhoneNumber)});
            }
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> EnterCode(AuthenticationMethod method, string token)
        {
            var model = new EnterCodeViewModel();
            
            var user =  await _userManager.GetUserAsync(User);
            
            if (method == AuthenticationMethod.Phone)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var phone = await _factorSettingsService.GetPhoneNumberAsync(userId);
                model.Message = phone;
            } 
            
            if (method == AuthenticationMethod.ChangePhone && !string.IsNullOrEmpty(token))
            {
                var phone = _encodingService.GetStringFromHex(token);
                model.Message = phone;
            }
            
            return View($"{method}Code", model);
        }


        [HttpGet]
        public async Task<IActionResult> ResendCode(AuthenticationMethod method, string token)
        {
            var user = await _userManager.GetUserAsync(User);
            var phoneNumber = _encodingService.GetStringFromHex(token);
            if (string.IsNullOrEmpty(phoneNumber))
            {
                phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            }

            if (method == AuthenticationMethod.Phone)
            {
                var code = await _userManager.GenerateTwoFactorTokenAsync(user, AuthenticationMethod.Phone.ToString());
                await SendSms(phoneNumber, code);
            }

            if (method == AuthenticationMethod.ChangePhone)
            {
                var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
                await SendSms(phoneNumber, code);
            }


            return RedirectToAction("EnterCode", new { method, token});
        }


        [HttpPost]
        public async Task<IActionResult> EnterCode(EnterCodeViewModel model, string token)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var code = model.Code
                    .Replace(" ", string.Empty)
                    .Replace("-", string.Empty);

                var isValidToken = false;
                if (model.Method == AuthenticationMethod.ChangePhone)
                {
                    var phoneNumber = _encodingService.GetStringFromHex(token); 
                    var result = await _userManager.ChangePhoneNumberAsync(user, phoneNumber, code);
                    isValidToken = result.Succeeded;
                }
                else
                {
                    isValidToken = await _userManager.VerifyTwoFactorTokenAsync(user, model.Method.ToString(), code);
                }
                
                if (isValidToken)
                {
                    await SetTwoFactorEnabledAsync(user, true, model.Method);
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("Code", "Verification code is not valid, please enter a valid code and try again");
            }
            return View($"{model.Method}Code", model);
        }


        [HttpGet("ChangePassword")]
        public async Task<IActionResult> ChangePasswordOld()
        {
            return NotFound();
        }

        private async Task SetTwoFactorEnabledAsync(IUser user, bool enabled, AuthenticationMethod method)
        {
            await _userManager.SetTwoFactorEnabledAsync(user, enabled);
            var userId = await _userManager.GetUserIdAsync(user);
            
            if (method == AuthenticationMethod.Phone)
            {
                await _factorSettingsService.SetPhoneNumberConfirmedAsync(userId, enabled);
            }
            
            if (method == AuthenticationMethod.Authenticator)
            {
                await _factorSettingsService.SetAuthenticatorConfirmedAsync(userId, enabled);
            }
        }

        private async Task SendSms(string phoneNumber, string code)
        {
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

        private async Task<string> GetUserId()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = await _userManager.GetUserIdAsync(user);
            return userId;
        }
    }
}
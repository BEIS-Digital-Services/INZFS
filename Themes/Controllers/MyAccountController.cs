using System.Collections.Generic;
using System.Threading.Tasks;
using INZFS.Theme.Models;
using INZFS.Theme.Services;
using INZFS.Theme.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OrchardCore.Users;

namespace INZFS.Theme.Controllers
{
    [Authorize]
    public class MyAccountController : Controller
    {
        private readonly UserManager<IUser> _userManager;
        private readonly ITwoFactorAuthenticationService _twoFactorAuthenticationService;
        private readonly IUserTwoFactorSettingsService _factorSettingsService;
        private readonly INotificationService _notificationService;
        private readonly NotificationOption _notificationOption;

        public MyAccountController(UserManager<IUser> userManager,
            ITwoFactorAuthenticationService twoFactorAuthenticationService,
            IUserTwoFactorSettingsService factorSettingsService, 
            INotificationService notificationService,
            IOptions<NotificationOption> notificationOption)
        {
            _userManager = userManager;
            _twoFactorAuthenticationService = twoFactorAuthenticationService;
            _factorSettingsService = factorSettingsService;
            _notificationService = notificationService;
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
                await SendSms(user);
                return RedirectToAction("EnterCode", new { method = AuthenticationMethod.Phone });
            }

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> EnterCode(AuthenticationMethod method)
        {
            var model = new EnterCodeViewModel();
            var user =  await _userManager.GetUserAsync(User);
            
            if (method == AuthenticationMethod.Phone)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var phone = await _factorSettingsService.GetPhoneNumberAsync(userId);
                model.Message = phone;
            }
            
            return View($"{method}Code", model);
        }

       
        [HttpPost]
        public async Task<IActionResult> EnterCode(EnterCodeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var code = model.Code
                    .Replace(" ", string.Empty)
                    .Replace("-", string.Empty);

                var isValidToken = await _userManager.VerifyTwoFactorTokenAsync(user, model.Method.ToString(), code);
                if (isValidToken)
                {
                    await SetTwoFactorEnabledAsync(user, true, model.Method);
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("Code", "Verification code is not valid, please enter a valid code and try again");
            }
            return View($"{model.Method}Code", model);
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

    }
}
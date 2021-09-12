using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using INZFS.Theme.Models;
using INZFS.Theme.Services;
using INZFS.Theme.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.Users;
using OrchardCore.Users.Events;

namespace INZFS.Theme.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly SignInManager<IUser> _signInManager;
        private readonly UserManager<IUser> _userManager;
        private readonly IUrlEncodingService _encodingService;
        private readonly IEnumerable<ILoginFormEvent> _accountEvents;
        private readonly INotificationService _notificationService;
        private readonly NotificationOption _notificationOption;
        private readonly ILogger<AccountController> _logger;


        public AccountController(SignInManager<IUser> signInManager,
            UserManager<IUser> userManager,
            IUrlEncodingService encodingService,
            IEnumerable<ILoginFormEvent> accountEvents,
            INotificationService notificationService,
            IOptions<NotificationOption> notificationOption,
            ILogger<AccountController> logger
            )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _encodingService = encodingService;
            _accountEvents = accountEvents;
            _notificationService = notificationService;
            _notificationOption = notificationOption.Value;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                returnUrl ??= Url.Content("~/");
                return LocalRedirect(returnUrl);
            }

            return View(new LoginViewModel());
        }
        
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.EmailAddress) ?? await _userManager.FindByNameAsync(model.EmailAddress);
                if (user != null)
                {
                    var checkResult = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
                    if (checkResult.Succeeded)
                    {
                        if (!await _userManager.IsEmailConfirmedAsync(user))
                        {
                            var idToken = _encodingService.GetHexFromString(model.EmailAddress);
                            return RedirectToAction("Success", "Registration", new { area = "INZFS.Theme", token = idToken, toverify = true });
                        }
                        var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, lockoutOnFailure: false);
                        if (result.RequiresTwoFactor)
                        {
                            return RedirectToAction("Select", "TwoFactor", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                        }
                        if (result.Succeeded)
                        {
                            _logger.LogInformation(1, "User logged in.");
                            await _accountEvents.InvokeAsync((e, model) => e.LoggedInAsync(user), model, _logger);
                            return LocalRedirect(returnUrl);
                        }
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                await _accountEvents.InvokeAsync((e, model) => e.LoggingInFailedAsync(model.EmailAddress), model, _logger);
            }

            return View(model);
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ForgotPassword(string returnUrl)
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                returnUrl ??= Url.Content("~/");
                return LocalRedirect(returnUrl);
            }

            return View(new ForgotPasswordViewModel());
        }
 
        
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {

                var user = await _userManager.FindByEmailAsync(model.EmailAddress);
                var tokenEmail = _encodingService.Encrypt(model.EmailAddress);
                if (user != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    token = _encodingService.Base64UrlEncode(token);
                    var resetUrl = Url.Action("ResetPassword", "Account", new { area = "INZFS.Theme", token = token, idToken = tokenEmail }, Request.Scheme);
                    await SendForgotPasswordEmailAsync(model.EmailAddress, resetUrl);
                }
              
                return RedirectToAction("ForgotPasswordConfirm", new {token = tokenEmail});
            }

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPasswordConfirm(string token)
        {
            var model = new ForgotPasswordViewModel();
            model.EmailAddress = _encodingService.Decrypt(token);
            return View(model);
        }
        
        
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword(string token, string idToken)
        {
            
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, string token, string idToken)
        {
            if (ModelState.IsValid)
            {
                var email =  _encodingService.Decrypt(idToken);
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    token = _encodingService.Base64UrlDecode(token);
                    var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("ResetPasswordSuccess");
                    }

                    AddIdentityErrors(result);
                }
                else
                {
                    _logger.LogInformation($"The user for reset password has not found for email={email}");
                    ModelState.AddModelError(string.Empty, "Invalid token.");
                }

                
            }

            return View(model);
        }


        [AllowAnonymous]
        public  IActionResult ResetPasswordSuccess()
        {
            return View();
        }

         [AllowAnonymous]
        public async Task<IActionResult> LogOff()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                await _signInManager.SignOutAsync();
                _logger.LogInformation(4, "User logged out.");
            }

            return Redirect("~/");
        }


        private async Task SendForgotPasswordEmailAsync(string email, string tokenLink)
        {
            await _notificationService.SendEmailAsync(email, _notificationOption.ForgotPasswordEmailTemplate,
                new Dictionary<string, object>() { { "Url", tokenLink } });
        }


        [AllowAnonymous]
        [HttpGet("login")]
        public async Task<IActionResult> LoginOld(string returnUrl)
        {
            return NotFound();
        } 
        
        [AllowAnonymous]
        [HttpGet("forgotpassword")]
        public async Task<IActionResult> ForgottenPasswordOld(string returnUrl)
        {
            return NotFound();
        }

        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
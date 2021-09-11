using System;
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
        private readonly ILogger<AccountController> _logger;


        public AccountController(SignInManager<IUser> signInManager,
            UserManager<IUser> userManager,
            IUrlEncodingService encodingService,
            IEnumerable<ILoginFormEvent> accountEvents,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _encodingService = encodingService;
            _accountEvents = accountEvents;
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
        public async Task<IActionResult> LogOff()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                await _signInManager.SignOutAsync();
                _logger.LogInformation(4, "User logged out.");
            }

            return Redirect("~/");
        }


        [AllowAnonymous]
        [HttpGet("login")]
        public async Task<IActionResult> LoginOld(string returnUrl)
        {
            return NotFound();
        }

    }
}
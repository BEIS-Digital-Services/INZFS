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
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using OrchardCore.Users;
using OrchardCore.Users.Models;

namespace INZFS.Theme.Controllers
{
    [Authorize]
    public class RegistrationController : Controller
    {
        private readonly UserManager<IUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly SignInManager<IUser> _signInManager;
        private readonly IUrlEncodingService _encodingService;

        public RegistrationController(UserManager<IUser> userManager, IEmailService emailService,
            SignInManager<IUser> signInManager, IUrlEncodingService encodingService)
        {
            _userManager = userManager;
            _emailService = emailService;
            _signInManager = signInManager;
            _encodingService = encodingService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Register(string returnUrl)
        {

            return View(new RegistrationViewModel());
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegistrationViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await FindUserAsync(model.Email);

                if (user != null)
                {
                    ModelState.AddModelError(string.Empty, AccountOptions.RegistrationUserAlreadyExists);
                }
                else
                {
                    user = new User()
                    {
                        UserId = Guid.NewGuid().ToString(),
                        UserName = model.Email,
                        Email = model.Email
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        var idToken = _encodingService.GetHexFromString(model.Email);
                        await SendEmail(user, model.Email, idToken, returnUrl);
                        return RedirectToAction("Success", new {token = idToken});
                    }
                    else
                    {
                        foreach (IdentityError error in result.Errors)
                        {
                            var safeErrorCode = error.Code ?? "";
                            ModelState.AddModelError(safeErrorCode, error.Description);
                        }
                    }
                }
            }

            return View("Register", model);
        }




        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Success(string token, bool toVerify = false)
        {
            var email = _encodingService.GetStringFromHex(token);
            return View(new RegistrationSuccessViewModel() {Email = email, VerificationRequired = toVerify});
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Resend(string token, bool toVerify = false)
        {
            var email = _encodingService.GetStringFromHex(token);
            var user = await FindUserAsync(email);
            if (user != null && !await _userManager.IsEmailConfirmedAsync(user))
            {
                await SendEmail(user, email, token, string.Empty);
            }

            return RedirectToAction("Success", new {token, toVerify});
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Verify(string token, string idtoken, string returnUrl = null)
        {

            if (User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("LogOff", "Account", new {area = "INZFS.Theme"});
            }

            var email = _encodingService.GetStringFromHex(idtoken);
            var user = await FindUserAsync(email);

            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);

                if (result.Succeeded)
                {
                    return View("Verified");
                }
               
            }

            ModelState.AddModelError("", "Invalid Token");
            return View("Verified");

        }


        private async Task<IUser> FindUserAsync(string userName)
        {
            var user = await _userManager.FindByEmailAsync(userName);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(userName);
            }

            return user;
        }

        private async Task SendEmail(IUser user, string email, string idToken, string returnUrl)
        {
            
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var tokenLink = Url.Action("Verify", "Registration",
                new {area = "INZFS.Theme", token = token, idtoken = idToken, returnUrl = returnUrl}, Request.Scheme);

            //TODO: the template id should be moved to DB or Orchard workflow
            await _emailService.SendEmailAsync(email, "3743ba10-430d-4e53-a620-a017d925dc08",
                new Dictionary<string, object>() {{"Url", tokenLink}});
        }

    }
}

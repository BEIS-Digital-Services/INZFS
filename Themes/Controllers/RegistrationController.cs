using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using INZFS.Theme.Attributes;
using INZFS.Theme.Models;
using INZFS.Theme.Services;
using INZFS.Theme.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Options;
using OrchardCore.Users;
using OrchardCore.Users.Models;

namespace INZFS.Theme.Controllers
{
    [Authorize]
    public class RegistrationController : Controller
    {
        private readonly UserManager<IUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly IUrlEncodingService _encodingService;
        private readonly IRegistrationManager _registrationManager;
        private readonly NotificationOption _notificationOption;

        public RegistrationController(UserManager<IUser> userManager, 
            INotificationService notificationService,
            IUrlEncodingService encodingService, 
            IOptions<NotificationOption> notificationOption, 
            IRegistrationManager registrationManager) 
        {
            _userManager = userManager;
            _notificationService = notificationService;
            _encodingService = encodingService;
            _registrationManager = registrationManager;
            _notificationOption = notificationOption.Value;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Register(string returnUrl)
        {
            return View(new RegistrationViewModel());
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                    user = BuildUser(model);

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        var idToken = _encodingService.GetHexFromString(model.Email);
                        await SendEmail(user, model.Email, idToken, returnUrl);
                        await _registrationManager.RegisterSignInAsync(user);
                        return RedirectToAction("Organisation");
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

        private static IUser BuildUser(RegistrationViewModel model)
        {
            var claims = new List<UserClaim>
            {
                new UserClaim()
                {
                    ClaimType = nameof(model.IsConsentedToUsePersonalInformation),
                    ClaimValue = model.IsConsentedToUsePersonalInformation.ToString()
                },
                new UserClaim()
                {
                    ClaimType = nameof(model.IsConsentedUseEmailAndSms),
                    ClaimValue = model.IsConsentedUseEmailAndSms.ToString()
                }
            };


            IUser user = new User()
            {
                UserId = Guid.NewGuid().ToString(),
                UserName = model.Email,
                Email = model.Email,
                UserClaims = claims
            };


            return user;
        }


        [RegistrationAuthorize]
        [HttpGet]
        public async Task<IActionResult> Organisation()
        {
            var user = await _registrationManager.GetRegistrationAuthenticationUserAsync();
            var userId = await _userManager.GetUserIdAsync(user);
            return View("Success", new RegistrationSuccessViewModel());
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

            if (user != null && !await _userManager.IsEmailConfirmedAsync(user))
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

        [AllowAnonymous]
        [HttpGet("Register")]
        public async Task<IActionResult> RegisterNotInUse(string returnUrl)
        {
            return NotFound();
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
            await _notificationService.SendEmailAsync(email, _notificationOption.EmailVerificationTemplate,
                new Dictionary<string, object>() {{"Url", tokenLink}});
        }

    }
}

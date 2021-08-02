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

        public RegistrationController(UserManager<IUser> userManager, IEmailService emailService, SignInManager<IUser> signInManager, IUrlEncodingService encodingService)
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
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var tokenLink = Url.Action("Verify", "Account", new { token = token, email = model.Email }, Request.Scheme);
                        await SendRegistrationEmail(model.Email, tokenLink);
                        return RedirectToAction("Success", new {token = _encodingService.GetHexFromString(model.Email)});
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
        public async Task<IActionResult> Success(string token)
        {
            var email = _encodingService.GetStringFromHex(token);
            return View(new RegistrationSuccessViewModel(){Email = email});
        }


        [HttpGet("Verify")]
        [AllowAnonymous]
        public async Task<IActionResult> Verify(string returnUrl = null)
        {

            return View();
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

        private async Task SendRegistrationEmail(string modelEmail, string tokenLink)
        {
            await _emailService.SendEmailAsync(modelEmail, "3743ba10-430d-4e53-a620-a017d925dc08", new Dictionary<string, object>(){{"Url", tokenLink } });
        }

    }


    public class AccountOptions
    {
       public static string RegistrationUserAlreadyExists = "User Name already taken";
    }

}

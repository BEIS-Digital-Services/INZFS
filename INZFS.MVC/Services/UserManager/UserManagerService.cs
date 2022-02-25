using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Users;
using OrchardCore.Users.Models;

namespace INZFS.MVC.Services.UserManager
{
    public class UserManagerService : ControllerBase, IUserManagerService
    {
        private readonly UserManager<IUser> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;

        public UserManagerService(UserManager<IUser> userManager, IHttpContextAccessor contextAccessor)
        {
            _userManager = userManager;
            _contextAccessor = contextAccessor;
        }
        public async Task<IUser> GetUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user;
        }
        public async Task<string> ReturnUserEmail(string userId)
        {
            var userEmail = await _userManager.GetEmailAsync(GetUserAsync(userId).Result);
            return userEmail;
        }
        public string GetUserId()
        {
            return _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}

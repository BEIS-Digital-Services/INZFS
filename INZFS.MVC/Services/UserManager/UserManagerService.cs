using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Users;
using OrchardCore.Users.Models;

namespace INZFS.MVC.Services.UserManager
{
    public class UserManagerService : IUserManagerService
    {
        private readonly UserManager<IUser> _userManager;

        public UserManagerService(UserManager<IUser> userManager)
        {
            _userManager = userManager;
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
    }
}

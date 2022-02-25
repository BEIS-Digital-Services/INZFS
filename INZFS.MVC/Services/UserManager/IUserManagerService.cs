using OrchardCore.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Services.UserManager
{
   public interface IUserManagerService
    {
        public Task<IUser> GetUserAsync(string userId);
        public Task<string> ReturnUserEmail(string userId);
        public string GetUserId();
    }
}

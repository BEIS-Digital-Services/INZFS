using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Services.UserManager
{
   public interface IUserManagerService
    {
        public Task<string> ReturnUserEmail(string userId);
    }
}

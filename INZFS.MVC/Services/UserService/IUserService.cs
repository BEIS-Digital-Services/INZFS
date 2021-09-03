using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Services.UserService
{
    public interface IUserService
    {
        public List<string> GetAllUsers();
    }
}

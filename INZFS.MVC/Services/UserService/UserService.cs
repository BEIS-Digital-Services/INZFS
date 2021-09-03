using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using Microsoft.AspNetCore.Identity;
using YesSql;
using OrchardCore.Users.Indexes;

namespace INZFS.MVC.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly UserManager<IUser> _userManager;
        private readonly ISession _session;

        public UserService(UserManager<IUser> userManager, ISession session)
        {
            _userManager = userManager;
            _session = session;
            
        }
        public List<string> GetAllUsers()
        {
            List<string> userList = new List<string>();
           var userIndexList =  _session.QueryIndex<UserIndex>().ListAsync().Result;
            foreach(UserIndex user in userIndexList)
            {
                var userName = user.NormalizedUserName;
                userList.Add(userName);
            }
            return userList;
        }

    }
}

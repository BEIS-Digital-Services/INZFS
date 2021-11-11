using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Users;

namespace INZFS.Theme.Validators
{
    public class CommonPasswordValidator : IPasswordValidator<IUser>
    {
        private readonly ICommonPasswordLists commonPasswordLists;

        public CommonPasswordValidator(ICommonPasswordLists commonPasswordLists)
        {
            this.commonPasswordLists = commonPasswordLists;
        }

        public async Task<IdentityResult> ValidateAsync(UserManager<IUser> manager, IUser user, string password)
        {
            var passwords = commonPasswordLists.GetPasswords();
            var commonPassword = passwords.FirstOrDefault(e => password.Contains((string) e, StringComparison.InvariantCultureIgnoreCase));
            
            if (IsValid(password, commonPassword))
            {
                return IdentityResult.Success;

            }

            return IdentityResult.Failed(new IdentityError
            {
                Code = "CommonPassword",
                Description = "Passwords must not be commonly used. Examples of commonly used passwords include 'password', '12345678' and 'qwerty1234'."
                //Description = "You must choose a password which is not commonly used. Examples of commonly used passwords include 'password' '12345678' and 'qwerty123'."
            });

        }

        private bool IsValid(string providedPassword, string commonPassword)
        {
            if (string.IsNullOrEmpty(providedPassword) || string.IsNullOrEmpty(commonPassword))
            {
                return true;
            }
            return providedPassword.Length > commonPassword.Length + 3;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace INZFS.Theme.Controllers
{
    [Authorize]
    public class RegistrationController : Controller
    {
        [AllowAnonymous]
        public async Task<IActionResult> Register(string returnUrl)
        {

            return View();
        }
    }
}

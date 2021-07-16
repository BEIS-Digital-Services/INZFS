using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using INZFS.Theme.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace INZFS.Theme.Controllers
{
    [Authorize]
    public class TwoFactorController : Controller
    {
        public async Task<IActionResult> Enable()
        {
            var model = new EnableAuthenticatorViewModel()
            {
                SharedKey = "fc3o gp5z w6e5 v4tm x7yc u7fw 35zx cyo3",
                AuthenticatorUri = "otpauth://totp/INZFS:Test3@hotmail.com?secret=FC3OGP5ZW6E5V4TMX7YCU7FW35ZXCYO3&issuer=INZFS&digits=6"
            };

            return View(model);
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace INZFS.MVC.Controllers
{
    public class HelpController : Controller
    {
        public IActionResult Accessibility()
        {
            return View();
        }
        public IActionResult PrivacyNotice()
        {
            return View();
        }
        public IActionResult Help()
        {
            return View();
        }
        public IActionResult Cookies()
        {
            return View();
        }

        public IActionResult RegisterToAttend()
        {
            return View();
        }

        public IActionResult QAndA()
        {
            return View();
        }
    }
}

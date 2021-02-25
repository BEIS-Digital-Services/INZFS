using Microsoft.AspNetCore.Mvc;

namespace INZFS.Theme.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

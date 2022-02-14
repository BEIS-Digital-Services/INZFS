using Microsoft.AspNetCore.Mvc;

namespace INZFS.MVC.Controllers
{
    public class AdminController : Controller
    {

        private readonly IContentRepository _contentRepository;

        public AdminController(IContentRepository contentRepository)
        {
            _contentRepository = contentRepository;
        }


        [HttpGet]
        public IActionResult GetApplicationsSearch(string companyName)
        {
            return View(null);
        }
    }

}



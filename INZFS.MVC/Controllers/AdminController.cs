using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using INZFS.MVC.Services;

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
        public async Task<IActionResult> GetApplicationsSearch(string companyName)
        {
            return View(null);
        }
    }

}



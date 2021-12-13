using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using INZFS.MVC.Services.Zip;
using Microsoft.AspNetCore.Authorization;

namespace INZFS.MVC.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IZipService _zipService;

        public ReportController (IZipService zipService)
        {
            _zipService = zipService;
        }

        public async Task<FileContentResult> DownloadApplication(string filetype)
        {
            var bytes = await _zipService.GetZipFileBytes(filetype, GetUserId());
            var applicationNumber = await _zipService.GetApplicationId(GetUserId());

            return File(bytes, "application/zip", $"{applicationNumber}.zip");
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}

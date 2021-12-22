using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using INZFS.MVC.Services.Zip;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace INZFS.MVC.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IZipService _zipService;
        private readonly ILogger _logger;

        public ReportController (IZipService zipService, ILogger<ReportController> logger)
        {
            _zipService = zipService;
            _logger = logger;
        }

        public async Task<FileContentResult> DownloadApplication(string filetype)
        {
            try
            {
                var bytes = await _zipService.GetZipFileBytes(filetype, GetUserId());
                var applicationNumber = await _zipService.GetApplicationId(GetUserId());

                return File(bytes, "application/zip", $"{applicationNumber}.zip");
            }
            catch(System.Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }

        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using INZFS.MVC.Services.PdfServices;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace INZFS.MVC.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private IWebHostEnvironment _env;
        public ReportController(IReportService reportService, IWebHostEnvironment env)
        {
            _reportService = reportService;
            _env = env;
        }
        [HttpGet]
        public async Task<FileContentResult> DownloadPdf()
        {
            string logoFilepath = Path.Combine(_env.WebRootPath, "assets", "images", "beis_logo.png");
            var reportContent = await _reportService.GeneratePdfReport(GetUserId(), logoFilepath);
            string type = "application/pdf";
            string name = $"Application Form {reportContent.ApplicationNumber}.pdf";

            return File(reportContent.FileContents, type, name);
        }
        public async Task<FileContentResult> GenerateOdt()
        {
            string logoFilepath = Path.Combine(_env.WebRootPath, "assets", "images", "beis_logo.png");
            var reportContent = await _reportService.GenerateOdtReport(GetUserId(), logoFilepath);
            string type = "application/vnd.oasis.opendocument.text";
            string name = $"Application Form {reportContent.ApplicationNumber}.odt";

            return File(reportContent.FileContents, type, name);
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
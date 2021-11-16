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
using System.IO.Compression;
using INZFS.MVC.Services;

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

        public async Task<FileContentResult> DownloadApplication()
        {
            var reportContent = await GetPdfContent();

            using (MemoryStream ms = new())
            {
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    var zipArchiveEntry = archive.CreateEntry($"Application Form {reportContent.ApplicationNumber}.pdf", CompressionLevel.Fastest);
                    using (var zipStream = zipArchiveEntry.Open()) zipStream.Write(reportContent.FileContents, 0, reportContent.FileContents.Length);
                }
                return File(ms.ToArray(), "application/zip", $"{reportContent.ApplicationNumber}.zip");
            }
        }

        private async Task<ReportContent> GetPdfContent()
        {
            string logoFilepath = Path.Combine(_env.WebRootPath, "assets", "images", "beis_logo.png");
            var reportContent = await _reportService.GeneratePdfReport(GetUserId(), logoFilepath);
            return reportContent;
        }

        [HttpGet]
        public async Task<FileContentResult> DownloadPdf()
        {
            var reportContent = await GetPdfContent();
            return File(reportContent.FileContents, "application/pdf", $"Application Form {reportContent.ApplicationNumber}.pdf");
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
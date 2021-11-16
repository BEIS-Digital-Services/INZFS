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
        private string _logoFilepath;
        public ReportController(IReportService reportService, IWebHostEnvironment env)
        {
            _reportService = reportService;
            _env = env;
            _logoFilepath = Path.Combine(_env.WebRootPath, "assets", "images", "beis_logo.png");

        }

        public async Task<FileContentResult> DownloadApplication(string filetype)
        {
            ReportContent reportContent;

            switch(filetype)
            {
                case "odt":
                    reportContent = await GetOdtContent();
                    break;
                case "pdf":
                    reportContent = await GetPdfContent();
                    break;
                default:
                    reportContent = await GetPdfContent();
                    filetype = "pdf";
                    break;
            }

            using (MemoryStream ms = new())
            {
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    var zipArchiveEntry = archive.CreateEntry($"Application Form {reportContent.ApplicationNumber}.{filetype}", CompressionLevel.Fastest);
                    using (var zipStream = zipArchiveEntry.Open()) zipStream.Write(reportContent.FileContents, 0, reportContent.FileContents.Length);
                }
                return File(ms.ToArray(), "application/zip", $"{reportContent.ApplicationNumber}.zip");
            }
        }

        private async Task<ReportContent> GetPdfContent()
        {
            var reportContent = await _reportService.GeneratePdfReport(GetUserId(), _logoFilepath);
            return reportContent;
        }

        private async Task<ReportContent> GetOdtContent()
        {
            var reportContent = await _reportService.GenerateOdtReport(GetUserId(), _logoFilepath);
            return reportContent;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
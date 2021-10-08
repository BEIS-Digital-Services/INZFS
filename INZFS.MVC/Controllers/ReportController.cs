using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using INZFS.MVC.Services.PdfServices;
using Microsoft.AspNetCore.Authorization;

namespace INZFS.MVC.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }
        [HttpGet]
        public async Task<FileContentResult> DownloadPdf()
        {
            var reportContent = await _reportService.GeneratePdfReport(GetUserId());
            string type = "application/pdf";
            string name = $"{reportContent.ApplicationNumber}.pdf";

            return File(reportContent.FileContents, type, name);
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        //public async Task<FileContentResult> GenerateOdt()
        //{
        //    byte[] bytes = await _reportService.GenerateOdtReport(GetUserId());
        //    string type = "application/vnd.oasis.opendocument.text";
        //    string name = "EEF_accessible_summary.odt";

        //    return File(bytes, type, name);

        //}
    }
}
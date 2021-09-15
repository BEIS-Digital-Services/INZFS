using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Controllers
{
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
        public async Task<FileContentResult> DownloadPdf(string applicationNumber)
        {
            string logoFilepath = Path.Combine(_env.WebRootPath, "assets", "images", "beis_logo.png");
            byte[] bytes = await _reportService.GeneratePdfReport(User.Identity.Name, logoFilepath);
            string type = "application/pdf";
            string name = $"{applicationNumber}.pdf";

            return File(bytes, type, name);
        }
        //public async Task<FileContentResult> GenerateOdt()
        //{
        //    byte[] bytes = await _reportService.GenerateOdtReport(User.Identity.Name);
        //    string type = "application/vnd.oasis.opendocument.text";
        //    string name = "EEF_accessible_summary.odt";

        //    return File(bytes, type, name);

        //}
    }
}
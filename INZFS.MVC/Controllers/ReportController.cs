using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }
        [HttpGet]
        public async Task<FileContentResult> DownloadPdf(string applicationNumber)
        {
            byte[] bytes = await _reportService.GeneratePdfReport(User.Identity.Name);
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
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
        public FileContentResult GeneratePdf(string title, string id)
        {
            var pdfFile = _reportService.GeneratePdfReport(title, id);
            return File(pdfFile, "application/octet-stream", $"eef_{title}_{id}.pdf");
        }
    }
}
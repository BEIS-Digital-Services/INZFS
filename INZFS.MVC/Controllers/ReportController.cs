using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Controllers
{
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }
        [HttpGet]
        public FileContentResult GeneratePdf()
        {
            var pdfFile = _reportService.GeneratePdfReport();
            return File(pdfFile, "application/octet-stream", "Placeholder.pdf");
        }
    }
}
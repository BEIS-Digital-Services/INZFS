using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace INZFS.MVC.Services.PdfServices
{
    public interface IReportService
    {
        public Task<ReportContent> GeneratePdfReport(string applicationAuthor);
        public Task<ReportContent> GenerateOdtReport(string applicationAuthor);
    }
}

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace INZFS.MVC.Services.PdfServices
{
    public interface IReportService
    {
        public Task<ReportContent> GeneratePdfReport(string applicationAuthor, string logoFilepath);
        public Task<ReportContent> GenerateOdtReport(string applicationAuthor, string logoFilepath);
    }
}

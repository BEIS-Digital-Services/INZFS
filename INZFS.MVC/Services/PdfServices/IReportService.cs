using INZFS.MVC.Models.Report;
using System.Threading.Tasks;

namespace INZFS.MVC.Services.PdfServices
{
    public interface IReportService
    {
        public Task<ReportContentModel> GeneratePdfReport(string userId, string logoFilepath);
        public Task<ReportContentModel> GenerateOdtReport(string userId, string logoFilepath);
    }
}

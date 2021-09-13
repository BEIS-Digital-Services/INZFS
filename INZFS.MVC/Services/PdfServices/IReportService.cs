using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public interface IReportService
{
    public Task<byte[]> GeneratePdfReport(string applicationAuthor);
    public Task<byte[]> GenerateOdtReport(string applicationAuthor);
}
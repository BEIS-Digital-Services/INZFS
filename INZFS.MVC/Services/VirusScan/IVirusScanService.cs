using Microsoft.AspNetCore.Http;

namespace INZFS.MVC.Services.VirusScan
{
    public interface IVirusScanService
    {
        public string ScanFile(IFormFile file, string cloudmersiveApiKey);
    }
}

using INZFS.MVC.Models.Application;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace INZFS.MVC.Services.FileUpload
{
    public interface IFileUploadService
    {
        public string ModifyFileName(string originalFileName);
        public Task<string> SaveFile(IFormFile file, string directoryName);
        public string Validate(IFormFile file, Page currentPage, bool virusScanningEnabled, string cloudmersiveApiKey);
        public Task<bool> CreateDirectory(string directoryName);
        public Task<bool> DeleteFile(string fileLocation);
    }
}

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Services.FileUpload
{
    public interface IFileUploadService
    {
        public string ModifyFileName(string originalFileName);
        public Task<string> SaveFile(IFormFile file, string directoryName);
        public bool IsValidFileExtension(IFormFile file);
        public Task<string> Validate(IFormFile file);
        public Task<bool> CreateDirectory(string directoryName);
    }
}

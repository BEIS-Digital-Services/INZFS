using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nClam;
using OrchardCore.Media;
using OrchardCore.FileStorage;
using System.IO;
using INZFS.MVC.Services.VirusScan;

namespace INZFS.MVC.Services.FileUpload
{
    public class FileUploadService : IFileUploadService
    {
        private const string UploadedFileFolderRelativePath = "GovUpload/UploadedFiles";
        private string[] permittedExtensions = { ".txt", ".pdf", ".xls", ".xlsx", ".doc", ".docx" };
        private readonly ClamClient _clam;
        private readonly IMediaFileStore _mediaFileStore;
        private readonly IVirusScanService _virusScanService;
        
        public FileUploadService(ClamClient clam, IMediaFileStore mediaFileStore, IVirusScanService virusScanService)
        {
            _clam = clam;
            _mediaFileStore = mediaFileStore;
            _virusScanService = virusScanService;
          
        }
        public string ModifyFileName(string originalFileName)
        {
            DateTime thisDate = DateTime.UtcNow;
            CultureInfo culture = new CultureInfo("pt-BR");
            DateTimeFormatInfo dtfi = culture.DateTimeFormat;
            dtfi.DateSeparator = "-";
            var newDate = thisDate.ToString("d", culture);
            var newTime = thisDate.ToString("FFFFFF", culture);
            var newFileName = newDate + newTime + originalFileName;
            return newFileName;
        }
        public async Task<string> SaveFile(IFormFile file, string directoryName)
        {
            var DirectoryCreated = await CreateDirectory(directoryName);

            if (DirectoryCreated)
            {
                var newFileName = ModifyFileName(file.FileName);
                var mediaFilePath = _mediaFileStore.Combine(directoryName, newFileName);
                using (var stream = file.OpenReadStream())
                {
                    await _mediaFileStore.CreateFileFromStreamAsync(mediaFilePath, stream);
                }

                var publicUrl = _mediaFileStore.MapPathToPublicUrl(mediaFilePath);
                return publicUrl;
            }
            else
            {
                return string.Empty;
            }
        }

        public bool IsValidFileExtension(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            return string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext);
        }

        public async Task<string> Validate(IFormFile file)
        {
            if (IsValidFileExtension(file))
            {
                return "Cannot accept files other than .doc, .docx, .xlx, .xlsx, .pdf";
            }
            if (file == null || file.Length == 0)
            {
                return "Empty file";
            }

            //TODO : Switch to virus scanning service
            var containsVirus = false; // await _virusScanService.ScanFile(file);
            if (containsVirus)
            {
                return "File contains virus";
            }

            return string.Empty;
        }
        public async Task<bool> CreateDirectory(string directoryName)
        {
            if (directoryName == null)
            {
                return false;
            }
            await _mediaFileStore.TryCreateDirectoryAsync(directoryName);
            return true;
        }

        public async Task<bool> DeleteFile(string fileLocation)
        {
            try
            {
                return await _mediaFileStore.TryDeleteFileAsync(fileLocation);
            }
            catch (Exception ex)
            {

                return false;
            }
        }
    }
}

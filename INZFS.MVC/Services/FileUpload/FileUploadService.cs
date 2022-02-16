using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using OrchardCore.Media;
using OrchardCore.FileStorage;
using System.IO;
using INZFS.MVC.Services.VirusScan;
using INZFS.MVC.Models.Application;

namespace INZFS.MVC.Services.FileUpload
{
    public class FileUploadService : IFileUploadService
    {
        private const string UploadedFileFolderRelativePath = "GovUpload/UploadedFiles";
        private string[] permittedExtensions = { ".ppt", ".pptx", ".pdf", ".xls", ".xlsx", ".doc", ".docx", "gif", "jpeg", "png" };
        private readonly IMediaFileStore _mediaFileStore;
        private readonly IVirusScanService _virusScanService;
        
        public FileUploadService(IMediaFileStore mediaFileStore, IVirusScanService virusScanService)
        {
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

        public bool IsValidFileExtension(IFormFile file, string permittedExtensions)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            return string.IsNullOrEmpty(ext) || !permittedExtensions.ToLower().Contains(ext.ToLower().Replace(".", "")); ;
        }

        public string Validate(IFormFile file, Page currentPage, bool virusScanningEnabled, string cloudmersiveApiKey)
        {

            if (file == null)
            {
                return currentPage.ErrorMessage;
            }

            if (file.Length == 0)
            {
                return "The selected file is empty";
            }

            if (IsValidFileExtension(file, currentPage.AcceptableFileExtensions))
            {
                return $"The selected file must be {currentPage.AcceptableFileExtensions}";
            }

            var maxSize = 20 * Math.Pow(1024, 2); // 20 MB
            if ((double)file.Length > maxSize)
            {
                return "The selected file must be smaller than 20MB";
            }
            string containsVirus = null;
            if (virusScanningEnabled)
            {
                var virusScanResult = _virusScanService.ScanFile(file, cloudmersiveApiKey);
                switch (virusScanResult)
                {
                    case "Virus":
                        containsVirus = "true";
                        break;
                    case "Clean":
                        containsVirus = "false";
                        break;
                    case "Exception":
                        containsVirus = "error";
                        break;
                }
            }
            switch (containsVirus)
            {
                case "true":
                    return "The selected file contains a virus";
                case "false":
                    return string.Empty;
                case "error":
                    return "There has been an error using the Virus scanning API";
                case null:
                    return "Virus scanning has not been enabled";
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
                Debug.WriteLine("The following exception was encountered: " + ex);
                return false;
            }
        }
    }
}

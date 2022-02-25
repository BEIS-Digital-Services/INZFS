using INZFS.MVC.Services.FileUpload;
using INZFS.MVC.Services.UserManager;
using INZFS.MVC.Services.Zip;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Services.AzureStorage
{
    public class AzureBlobService : IAzureBlobService
    {
        public static IFileUploadService _fileUploadService;
        public static IUserManagerService _userManagerService;
        public static IZipService _zipService;

        public AzureBlobService(IFileUploadService fileUploadService, IUserManagerService userManagerService, IZipService zipService)
        {
            _fileUploadService = fileUploadService;
            _userManagerService = userManagerService;
            _zipService = zipService;
        }
        public async Task<string> AddFileToBlobStorage(FormFile file)
        {
            try
            {
                var publicUrl = await _fileUploadService.SaveFile(file, "Submitted Applications");
                return publicUrl;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public async Task<bool> AddApplicationToBlobStorage()
        {
            string userId = _userManagerService.GetUserId();
            string applicationId = await _zipService.GetApplicationId(userId);
            byte[] zipFileBytes = await _zipService.GetZipFileBytes("pdf", userId, true);
            string companyName = await _zipService.GetApplicationCompanyName(userId);
            string name = $"_{companyName}_{applicationId}.zip";

            MemoryStream ms = new(zipFileBytes);
            FormFile file = new(ms, 0, zipFileBytes.Length, name, name);

            var url = await AddFileToBlobStorage(file);

            return url != null ? true : false;
        }
    }
}

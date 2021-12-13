using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using INZFS.MVC.Services.PdfServices;
using INZFS.MVC.Services.Zip;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.IO.Compression;
using INZFS.MVC.Services;
using System.Text.Json;
using OrchardCore.Media;
using OrchardCore.FileStorage;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;

namespace INZFS.MVC.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private IWebHostEnvironment _env;
        private string _logoFilepath;
        private readonly IContentRepository _contentRepository;
        private readonly IMediaFileStore _mediaFileStore;
        private readonly IConfiguration _configuration;
        private readonly IZipService _zipService;

        public ReportController(IReportService reportService, IZipService zipService,IWebHostEnvironment env, IContentRepository contentRepository, IMediaFileStore mediaFileStore, IConfiguration configuration)
        {
            _reportService = reportService;
            _zipService = zipService;
            _env = env;
            _logoFilepath = Path.Combine(_env.WebRootPath, "assets", "images", "beis_logo.png");
            _contentRepository = contentRepository;
            _mediaFileStore = mediaFileStore;
            _configuration = configuration;
        }

        public async Task<FileContentResult> DownloadApplication(string filetype)
        {
            var bytes = await _zipService.GetZipFileBytes(filetype, GetUserId());
            var applicationNumber = _zipService.GetApplicationId(GetUserId());

            return File(bytes, "application/zip", $"{applicationNumber}.zip");
        }

        public async Task<FileContentResult> DownloadPdf()
        {
            var reportContent = await GetPdfContent();
            string type = "application/pdf";
            string name = $"Application Form {reportContent.ApplicationNumber}.pdf";

            return File(reportContent.FileContents, type, name);
        }

        public async Task<FileContentResult> DownloadOdt()
        {
            var reportContent = await GetOdtContent();
            string type = "application/vnd.oasis.opendocument.text";
            string name = $"Application Form {reportContent.ApplicationNumber}.odt";

            return File(reportContent.FileContents, type, name);
        }

        private async Task<BinaryData> GetFileFromBlobStorage(UploadedFile file)
        {
            try
            {
                string connectionString = _configuration["AzureBlobStorage"];
                string containerName = Environment.GetEnvironmentVariable("OrchardCore__OrchardCore_Shells_Azure__ContainerName");
                string blobName = file.FileLocation.Replace("/media", "EEF");

                var blob = new BlobClient(connectionString, containerName, blobName).DownloadContent().Value;
                return blob.Content;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error getting file {file.Name} from Blob Storage: " + e.Message);
                return null;
            }
        }

        private async Task<ReportContent> GetPdfContent()
        {
            var reportContent = await _reportService.GeneratePdfReport(GetUserId(), _logoFilepath);
            return reportContent;
        }

        private async Task<ReportContent> GetOdtContent()
        {
            var reportContent = await _reportService.GenerateOdtReport(GetUserId(), _logoFilepath);
            return reportContent;
        }

        private async Task<List<UploadedFile>> GetApplicationFiles()
        {
            var uploadedFiles = new List<UploadedFile>();

            var applicationContent = await _contentRepository.GetApplicationContent(GetUserId());
            var uploadedFileFields = applicationContent.Fields.FindAll(field => field.AdditionalInformation != null);

            foreach(var field in uploadedFileFields)
            {
                UploadedFile file = JsonSerializer.Deserialize<UploadedFile>(field.AdditionalInformation);
                uploadedFiles.Add(file);
            }
            return uploadedFiles;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using INZFS.MVC.Services.PdfServices;
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

        public ReportController(IReportService reportService, IWebHostEnvironment env, IContentRepository contentRepository, IMediaFileStore mediaFileStore, IConfiguration configuration)
        {
            _reportService = reportService;
            _env = env;
            _logoFilepath = Path.Combine(_env.WebRootPath, "assets", "images", "beis_logo.png");
            _contentRepository = contentRepository;
            _mediaFileStore = mediaFileStore;
            _configuration = configuration;
        }

        public async Task<FileContentResult> DownloadApplication(string filetype)
        {
            ReportContent reportContent;

            switch(filetype)
            {
                case "odt":
                    reportContent = await GetOdtContent();
                    break;
                case "pdf":
                    reportContent = await GetPdfContent();
                    break;
                default:
                    reportContent = await GetPdfContent();
                    filetype = "pdf";
                    break;
            }

            List<UploadedFile> uploadedFiles = await GetApplicationFiles();

            using (MemoryStream ms = new())
            {
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    var applicationForm = archive.CreateEntry($"Application Form {reportContent.ApplicationNumber}.{filetype}", CompressionLevel.Fastest);
                    using (var zipStream = applicationForm.Open()) zipStream.Write(reportContent.FileContents, 0, reportContent.FileContents.Length);

                    string env = _env.EnvironmentName;

                    foreach(var file in uploadedFiles)
                    {
                        BinaryData binaryData = await GetFileFromBlobStorage(file);
                        var stream = binaryData.ToStream();

                        byte[] bytes;
                        using (var streamReader = new MemoryStream())
                        {
                            stream.CopyTo(streamReader);
                            bytes = streamReader.ToArray();
                        }

                        var fileToArchive = archive.CreateEntry($"{file.Name}", CompressionLevel.Fastest);
                        using (var zipStream = fileToArchive.Open()) zipStream.Write(bytes, 0, bytes.Length);
                    }
                }

                return File(ms.ToArray(), "application/zip", $"{reportContent.ApplicationNumber}.zip");
            }
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
                //TODO: Remove this hardcoded container name to env var
                string containerName = "appdatasandbox";
                string blobName = file.FileLocation;

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

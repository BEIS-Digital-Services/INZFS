using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Media;
using System.IO;
using System.IO.Compression;
using INZFS.MVC.Services.PdfServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using System.Text.Json;

namespace INZFS.MVC.Services.Zip
{
    public class ZipService : IZipService
    {
        private readonly IReportService _reportService;
        private IWebHostEnvironment _env;
        private string _logoFilepath;
        private readonly IContentRepository _contentRepository;
        private readonly IMediaFileStore _mediaFileStore;
        private readonly IConfiguration _configuration;
        private string _userId;

        public ZipService(IReportService reportService, IWebHostEnvironment env, IContentRepository contentRepository, IMediaFileStore mediaFileStore, IConfiguration configuration)
        {
            _reportService = reportService;
            _env = env;
            _logoFilepath = Path.Combine(_env.WebRootPath, "assets", "images", "beis_logo.png");
            _contentRepository = contentRepository;
            _mediaFileStore = mediaFileStore;
            _configuration = configuration;
        }
        public async Task<byte[]> GetZipFileBytes(string filetype, string userId)
        {
            ReportContent reportContent;
            _userId = userId;

            switch (filetype)
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

                    foreach (var file in uploadedFiles)
                    {
                        BinaryData binaryData = await GetFileFromBlobStorage(file);
                        var stream = binaryData.ToStream();

                        byte[] bytes;
                        using (var streamReader = new MemoryStream())
                        {
                            stream.CopyTo(streamReader);
                            bytes = streamReader.ToArray();
                        }

                        var fileToArchive = archive.CreateEntry($"Uploaded Files/{file.Name}", CompressionLevel.Fastest);
                        using (var zipStream = fileToArchive.Open()) zipStream.Write(bytes, 0, bytes.Length);
                    }

                    //LOCAL DEVELOPMENT WORKAROUND
                    //foreach(var file in uploadedFiles)
                    //{
                    //    string path = _mediaFileStore.NormalizePath("/App_Data/Sites/Default" + file.FileLocation);
                    //    var zipArchiveEntry = archive.CreateEntryFromFile(path, Path.Combine("Uploaded Documents", file.Name));
                    //}
                }

                return ms.ToArray();
            }
        }

        private async Task<BinaryData> GetFileFromBlobStorage(UploadedFile file)
        {
            try
            {
                string connectionString = Environment.GetEnvironmentVariable("OrchardCore__OrchardCore_Shells_Azure__ConnectionString");
                string containerName = Environment.GetEnvironmentVariable("OrchardCore__OrchardCore_Shells_Azure__ContainerName");
                string basePath = Environment.GetEnvironmentVariable("OrchardCore__OrchardCore_Shells_Azure__BasePath");
                string blobName = file.FileLocation.Replace("/media", basePath);

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
            var reportContent = await _reportService.GeneratePdfReport(_userId, _logoFilepath);
            return reportContent;
        }

        private async Task<ReportContent> GetOdtContent()
        {
            var reportContent = await _reportService.GenerateOdtReport(_userId, _logoFilepath);
            return reportContent;
        }

        private async Task<List<UploadedFile>> GetApplicationFiles()
        {
            var uploadedFiles = new List<UploadedFile>();

            var applicationContent = await _contentRepository.GetApplicationContent(_userId);
            var uploadedFileFields = applicationContent.Fields.FindAll(field => field.AdditionalInformation != null);

            foreach (var field in uploadedFileFields)
            {
                UploadedFile file = JsonSerializer.Deserialize<UploadedFile>(field.AdditionalInformation);
                uploadedFiles.Add(file);
            }
            return uploadedFiles;
        }

        public async Task<string> GetApplicationId(string userId)
        {
            var applicationContent = await _contentRepository.GetApplicationContent(_userId);
            return applicationContent.ApplicationNumber;
        }
    }
}

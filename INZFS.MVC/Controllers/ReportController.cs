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

        public ReportController(IReportService reportService, IWebHostEnvironment env, IContentRepository contentRepository, IMediaFileStore mediaFileStore)
        {
            _reportService = reportService;
            _env = env;
            _logoFilepath = Path.Combine(_env.WebRootPath, "assets", "images", "beis_logo.png");
            _contentRepository = contentRepository;
            _mediaFileStore = mediaFileStore;
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

                    foreach(var file in uploadedFiles)
                    {
                        //var zipArchiveEntry = archive.CreateEntryFromFile(Path.Combine(_env.WebRootPath, file.FileLocation), file.Name);
                        var stream = _mediaFileStore.GetFileStreamAsync(file.FileLocation);
                    }
                }

                return File(ms.ToArray(), "application/zip", $"{reportContent.ApplicationNumber}.zip");
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
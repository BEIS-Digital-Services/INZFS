using INZFS.MVC.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.FileStorage;
using OrchardCore.Media;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using nClam;
using System.Collections.Generic;
using INZFS.MVC.Models;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;

namespace INZFS.MVC.Controllers
{
    public class GovUploadController : Controller
    {
        private const string TestFileRelativePath = "GovUploadTest/TestFile.txt";

        private const string UploadedFileFolderRelativePath = "GovUpload/UploadedFiles";

        private readonly IMediaFileStore _mediaFileStore;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer H;
        private readonly IGovFileStore _govFileStore;
        private readonly ClamClient _clam;
        private readonly ILogger _logger;
        private string[] permittedExtensions = { ".txt", ".pdf", ".jpg" };
        private readonly MediaOptions _mediaOptions;
        private readonly HashSet<string> _allowedFileExtensions;
        private readonly IMediaNameNormalizerService _mediaNameNormalizerService;

        public GovUploadController(
            IMediaFileStore mediaFileStore,
            INotifier notifier,
            IHtmlLocalizer<GovUploadController> htmlLocalizer,
            IGovFileStore govFileStore,
            ClamClient clam,
            ILogger<GovUploadController> logger)
        {
            _mediaFileStore = mediaFileStore;
            _notifier = notifier;
            _govFileStore = govFileStore;
            H = htmlLocalizer;
            _clam = clam;
            _logger = logger;
        }

        public async Task<string> CreateFileInMediaFolder()
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("Hello world!")))
            {
                await _mediaFileStore.CreateFileFromStreamAsync(TestFileRelativePath, stream, true);
            }

            var fileInfo = await _mediaFileStore.GetFileInfoAsync(TestFileRelativePath);

            var publicUrl = _mediaFileStore.MapPathToPublicUrl(TestFileRelativePath);

            return $"Successfully created file! File size: {fileInfo.Length} bytes. Public URL: {publicUrl}";
        }
        public async Task<string> ReadFileFromMediaFolder()
        {
        
            if (await _mediaFileStore.GetFileInfoAsync(TestFileRelativePath) == null)
            {
                return "Create the file first!";
            }

            using var stream = await _mediaFileStore.GetFileStreamAsync(TestFileRelativePath);
            using var streamReader = new StreamReader(stream);
            var content = await streamReader.ReadToEndAsync();


            return $"File content: {content}";
        }
        public ActionResult UploadFileToMedia() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> UploadFileToMediaPost(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            {
                return NotFound();
            }
            
            var mediaFilePath = _mediaFileStore.Combine(UploadedFileFolderRelativePath, file.FileName);

            var filePathSplit = mediaFilePath.Split("/");
            var fileLastName = filePathSplit.ElementAt(6);
            var fileLast = filePathSplit.Last();
            
            using (var stream = file.OpenReadStream())
            {
                await _mediaFileStore.CreateFileFromStreamAsync(fileLast, stream);
            }

            var log = new List<ScanResult>();

            
                if (file.Length > 0)
                {
                    var extension = file.FileName.Contains('.')
                        ? file.FileName.Substring(file.FileName.LastIndexOf('.'), file.FileName.Length - file.FileName.LastIndexOf('.'))
                        : string.Empty;
                    var newfile = new Models.File
                    {
                        Name = $"{Guid.NewGuid()}{extension}",
                        Alias = file.FileName,
                        Region = "uk-sourth",
                        //Bucket = BUCKET_NAME,
                        ContentType = file.ContentType,
                        Size = file.Length,
                        Uploaded = DateTime.UtcNow,
                    };
                    var ping = await _clam.PingAsync();

                    if (ping)
                    {
                        _logger.LogInformation("Successfully pinged the ClamAV server.");
                        var result = await _clam.SendAndScanFileAsync(file.OpenReadStream());

                        newfile.ScanResult = result.Result.ToString();
                        newfile.Infected = result.Result == ClamScanResults.VirusDetected;
                        newfile.Scanned = DateTime.UtcNow;
                        if (result.InfectedFiles != null)
                        {
                            foreach (var infectedFile in result.InfectedFiles)
                            {
                                newfile.Viruses.Add(new Virus
                                {
                                    Name = infectedFile.VirusName
                                });
                            }
                        }
                        var metaData = new Dictionary<string, string>
                        {
                            { "av-status", result.Result.ToString() },
                            { "av-timestamp", DateTime.UtcNow.ToString() },
                            { "alias", newfile.Alias }
                        };

                        var scanResult = new ScanResult()
                        {
                            FileName = file.FileName,
                            Result = result.Result.ToString(),
                            Message = result.InfectedFiles?.FirstOrDefault()?.VirusName,
                            RawResult = result.RawResult
                        };
                        log.Add(scanResult);
                    }
                    else
                    {
                        _logger.LogWarning("Wasn't able to connect to the ClamAV server.");
                    }
                }
            

            var model = new UploadFilesViewModel
            {
                Results = log
            };

            return RedirectToAction(nameof(UploadFileToMedia));
        }

            
        public async Task<string> CreateFileInCustomFolder()
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("Gov file storage!")))
            {
                await _govFileStore.CreateFileFromStreamAsync(TestFileRelativePath, stream, true);
            }

            var fileInfo = await _govFileStore.GetFileInfoAsync(TestFileRelativePath);

            return $"Successfully created file in the custom file storage! File size: {fileInfo.Length} bytes.";
        }
    }
}
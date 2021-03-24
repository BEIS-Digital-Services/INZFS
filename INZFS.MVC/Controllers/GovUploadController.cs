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

        public GovUploadController(
            IMediaFileStore mediaFileStore,
            INotifier notifier,
            IHtmlLocalizer<GovUploadController> htmlLocalizer,
            IGovFileStore govFileStore)
        {
            _mediaFileStore = mediaFileStore;
            _notifier = notifier;
            _govFileStore = govFileStore;
            H = htmlLocalizer;
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

        [HttpPost, ActionName(nameof(UploadFileToMedia)), ValidateAntiForgeryToken]
        public async Task<ActionResult> UploadFileToMediaPost(IFormFile file)
        {
            var mediaFilePath = _mediaFileStore.Combine(UploadedFileFolderRelativePath, file.FileName);

            using (var stream = file.OpenReadStream())
            {
                await _mediaFileStore.CreateFileFromStreamAsync(mediaFilePath, stream);
            }

            _notifier.Information(H["Successfully uploaded file!"]);

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
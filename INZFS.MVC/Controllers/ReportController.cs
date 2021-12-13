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

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}

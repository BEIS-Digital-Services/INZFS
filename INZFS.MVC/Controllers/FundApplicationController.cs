using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nClam;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using INZFS.MVC.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Settings;
using YesSql;
using Microsoft.AspNetCore.Authorization;
using INZFS.MVC.Models;
using INZFS.MVC.Forms;
using System.IO;
using Microsoft.AspNetCore.Http;
using OrchardCore.Media;
using OrchardCore.FileStorage;
using Microsoft.Extensions.Logging;

namespace INZFS.MVC.Controllers
{
    [Authorize]
    public class FundApplicationController : Controller
    {
        private const string contentType = "ProposalSummaryPart";

        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IHtmlLocalizer H;
        private readonly IMediaFileStore _mediaFileStore;
        private readonly dynamic New;
        private readonly INotifier _notifier;
        private readonly YesSql.ISession _session;
        private readonly ISiteService _siteService;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly INavigation _navigation;
        private const string UploadedFileFolderRelativePath = "GovUpload/UploadedFiles";
        private string[] permittedExtensions = { ".txt", ".pdf", ".jpg" };
        private readonly ILogger _logger;
        private readonly ClamClient _clam;

        public FundApplicationController(ILogger<FundApplicationController> logger, ClamClient clam, IContentManager contentManager, IMediaFileStore mediaFileStore, IContentDefinitionManager contentDefinitionManager,
            IContentItemDisplayManager contentItemDisplayManager, IHtmlLocalizer<FundApplicationController> htmlLocalizer,
            INotifier notifier, YesSql.ISession session, IShapeFactory shapeFactory, ISiteService siteService,
            IUpdateModelAccessor updateModelAccessor, INavigation navigation)
        {
            _contentManager = contentManager;
            _mediaFileStore = mediaFileStore;
            _contentDefinitionManager = contentDefinitionManager;
            _contentItemDisplayManager = contentItemDisplayManager;
            _notifier = notifier;
            _session = session;
            _siteService = siteService;
            _updateModelAccessor = updateModelAccessor;
            _clam = clam;
            _logger = logger;
            H = htmlLocalizer;
            New = shapeFactory;
            _navigation = navigation;
        }

        [HttpGet]
        public async Task<IActionResult> Section(string pagename, string id)
        {
            pagename = pagename.ToLower().Trim();

            if (pagename == "summary")
            {
                var query = _session.Query<ContentItem, ContentItemIndex>();
                query = query.With<ContentItemIndex>(x => x.ContentType == "ProjectSummaryPart"
                || x.ContentType == "ProjectDetailsPart"
                || x.ContentType == "OrgFundingPart");
                //query = query.With<ContentItemIndex>(x => _navigation.PageList().Any(p => p.ContentType == x.ContentType));
                query = query.With<ContentItemIndex>(x => x.Published);
                query = query.With<ContentItemIndex>(x => x.Author == User.Identity.Name);

                var items = await query.ListAsync();
                var projectSummary = items.FirstOrDefault(item => item.ContentType == "ProjectSummaryPart");
                var projectSummaryPart = projectSummary?.ContentItem.As<ProjectSummaryPart>();

                var projectDetails = items.FirstOrDefault(item => item.ContentType == "ProjectDetailsPart");
                var projectDetailsPart = projectDetails?.ContentItem.As<ProjectDetailsPart>();

                var funding = items.FirstOrDefault(item => item.ContentType == "OrgFundingPart");
                var fundingPart = funding?.ContentItem.As<OrgFundingPart>();

                var model = new SummaryViewModel
                {
                    ProjectSummaryViewModel = new ProjectSummaryViewModel
                    {
                        ProjectName = projectSummaryPart.ProjectName,
                        Day = projectSummaryPart.Day,
                        Month = projectSummaryPart.Month,
                        Year = projectSummaryPart.Year,
                        fileUploadPath = projectSummaryPart.fileUploadPath
                    },
                    ProjectDetailsViewModel = new ProjectDetailsViewModel
                    {
                        Summary = projectDetailsPart.Summary,
                        Timing = projectDetailsPart.Timing
                    },
                    OrgFundingViewModel = new OrgFundingViewModel
                    {
                        NoFunding = fundingPart.NoFunding,
                        Funders = fundingPart.Funders,
                        FriendsAndFamily = fundingPart.FriendsAndFamily,
                        PublicSectorGrants = fundingPart.PublicSectorGrants,
                        AngelInvestment = fundingPart.AngelInvestment,
                        VentureCapital = fundingPart.VentureCapital,
                        PrivateEquity = fundingPart.PrivateEquity,
                        StockMarketFlotation = fundingPart.StockMarketFlotation
                    },
                };

                return View("Summary", model);
            }

            var page = _navigation.GetPage(pagename);
            if (page == null)
            {
                return NotFound();
            }
            else
            {
                return await Create(page.ContentType);
            }
        }
        public async Task<IActionResult> Index(PagerParameters pagerParameters) //ListContentsViewModel model, 
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            var query = _session.Query<ContentItem, ContentItemIndex>();
            var newContentType = contentType;
            query = query.With<ContentItemIndex>(x => x.ContentType == newContentType);
            query = query.With<ContentItemIndex>(x => x.Published);
            //query = query.With<ContentItemIndex>(x => x.Author == User.Identity.Name);

            query = query.OrderByDescending(x => x.PublishedUtc);

            var maxPagedCount = siteSettings.MaxPagedCount;
            if (maxPagedCount > 0 && pager.PageSize > maxPagedCount)
                pager.PageSize = maxPagedCount;

            var routeData = new RouteData();
            var pagerShape = (await New.Pager(pager)).TotalItemCount(maxPagedCount > 0 ? maxPagedCount : await query.CountAsync()).RouteData(routeData);

            var pageOfContentItems = await query.Skip(pager.GetStartIndex()).Take(pager.PageSize).ListAsync();
            IEnumerable<ContentItem> model = await query.ListAsync();

            // Prepare the content items Summary Admin shape
            var contentItemSummaries = new List<dynamic>();
            foreach (var contentItem in pageOfContentItems)
            {
                contentItemSummaries.Add(await _contentItemDisplayManager.BuildDisplayAsync(contentItem, _updateModelAccessor.ModelUpdater, "Summary"));
            }

            var viewModel = new ListContentsViewModel
            {
                ContentItems = contentItemSummaries,
                Pager = pagerShape
                //Options = model.Options
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Create(string contentType)
        {
            if (String.IsNullOrWhiteSpace(contentType))
            {
                return NotFound();
            }

            var query = _session.Query<ContentItem, ContentItemIndex>();
            query = query.With<ContentItemIndex>(x => x.ContentType == contentType);
            query = query.With<ContentItemIndex>(x => x.Published);
            query = query.With<ContentItemIndex>(x => x.Author == User.Identity.Name);

            var records = await query.CountAsync();
            if (records > 0)
            {
                var existingContentItem = await query.FirstOrDefaultAsync();
                return await Edit(existingContentItem.ContentItemId, contentType);
            }
            var newContentItem = await _contentManager.NewAsync(contentType);
            var model = await _contentItemDisplayManager.BuildEditorAsync(newContentItem, _updateModelAccessor.ModelUpdater, true);

            return View("Create", model);

        }

        [HttpPost, ActionName("Create")]
        [FormValueRequired("submit.Publish")]
        public async Task<IActionResult> CreateAndPublishPOST([Bind(Prefix = "submit.Publish")] string submitPublish, string returnUrl, string contentType)
        {
            var stayOnSamePage = submitPublish == "submit.PublishAndContinue";
            var dummyContent = await _contentManager.NewAsync(contentType);

            returnUrl = $"process/person-summary";
            return await CreatePOST(contentType, returnUrl, stayOnSamePage, async contentItem =>
            {
                await _contentManager.PublishAsync(contentItem);

                var currentContentType = contentItem.ContentType;
            });
        }

        private async Task<IActionResult> CreatePOST(string id, string returnUrl, bool stayOnSamePage, Func<ContentItem, Task> conditionallyPublish)
        {
            var contentItem = await _contentManager.NewAsync(id);


            contentItem.Owner = User.Identity.Name;

            var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, true);

            if (!ModelState.IsValid)
            {
                _session.Cancel();
                return View("Create", model);
            }

            await _contentManager.CreateAsync(contentItem, VersionOptions.Draft);

            await conditionallyPublish(contentItem);

            var page = _navigation.GetNextPageByContentType(contentItem.ContentType);
            if (page == null)
            {
                return NotFound();
            }
            return RedirectToAction("section", new { pagename = page.Name });
        }

   
        [HttpPost]
        public async Task<IActionResult> Save(IFormFile file,  string pagename)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            {
                return BadRequest();
            }

            try
            {
                 var containsVirus = await ScanFile(file);
                if (!containsVirus)
                {
                    pagename = pagename.ToLower().Trim();

                    var query = _session.Query<ContentItem, ContentItemIndex>();
                    query = query.With<ContentItemIndex>(x => x.ContentType == "ProjectSummaryPart"
                    || x.ContentType == "ProjectDetailsPart"
                    || x.ContentType == "OrgFundingPart");
                    query = query.With<ContentItemIndex>(x => x.Published);
                    query = query.With<ContentItemIndex>(x => x.Author == User.Identity.Name);

                    var items = await query.ListAsync();
                    var projectSummary = items.FirstOrDefault(item => item.ContentType == "ProjectSummaryPart");
                    var projectSummaryPart = projectSummary?.ContentItem.As<ProjectSummaryPart>();

                    if (file == null || file.Length == 0)
                    {
                        return Content("File not selected");
                    }
                    else
                    {
                        var mediaFilePath = _mediaFileStore.Combine(UploadedFileFolderRelativePath, file.FileName);

                        using (var stream = file.OpenReadStream())
                        {
                            await _mediaFileStore.CreateFileFromStreamAsync(mediaFilePath, stream);
                        }

                        _notifier.Information(H["Successfully uploaded file!"]);
                        var publicUrl = _mediaFileStore.MapPathToPublicUrl(mediaFilePath);
                        projectSummaryPart.fileUploadPath = publicUrl;
                        var page = _navigation.GetPage(pagename);
                        if (page == null)
                        {
                            return NotFound();
                        }
                        else
                        {
                            return await Edit(projectSummary.ContentItemId, projectSummary.ContentType);
                        }
                    }
                }
                else
                {
                    return BadRequest();
                }
                
            }
            catch
            {
                return BadRequest();
            }
           
        }

        public async Task<bool> ScanFile(IFormFile file)
        {
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
                        } return false;
                    }
                    else
                    {
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
                    return true;
                }
                else
                {
                    _logger.LogWarning("Wasn't able to connect to the ClamAV server.");
                    return false;
                }

            }
            return false;
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string contentItemId, string contentName)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (contentItem == null)
                return NotFound();

            var model = await _contentItemDisplayManager.BuildEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false);

            return View("Edit", model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Publish")]
        public async Task<IActionResult> EditAndPublishPOST(string contentItemId, [Bind(Prefix = "submit.Publish")] string submitPublish, string returnUrl)
        {
            var stayOnSamePage = submitPublish == "submit.PublishAndContinue";

            var content = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (content == null)
            {
                return NotFound();
            }

            return await EditPOST(contentItemId, returnUrl, stayOnSamePage, async contentItem =>
            {
                await _contentManager.PublishAsync(contentItem);

                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            });
        }

        private async Task<IActionResult> EditPOST(string contentItemId, string returnUrl, bool stayOnSamePage, Func<ContentItem, Task> conditionallyPublish)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.DraftRequired);

            if (contentItem == null)
            {
                return NotFound();
            }

            var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false);
            if (!ModelState.IsValid)
            {
                _session.Cancel();
                return View("Edit", model);
            }


            _session.Save(contentItem);

            await conditionallyPublish(contentItem);

            var page = _navigation.GetNextPageByContentType(contentItem.ContentType);
            if (page == null)
            {
                return NotFound();
            }
            return RedirectToAction("section", new { pagename = page.Name });
        }

        public async Task<IActionResult> Remove(string contentItemId, string returnUrl)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (contentItem != null)
            {
                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                await _contentManager.RemoveAsync(contentItem);

                _notifier.Success(string.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? H["That content has been removed."]
                    : H["That {0} has been removed.", typeDefinition.DisplayName]);
            }

            return Url.IsLocalUrl(returnUrl) ? (IActionResult)LocalRedirect(returnUrl) : RedirectToAction("Index");
        }
    }
}

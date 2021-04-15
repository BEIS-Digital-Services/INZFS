using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using INZFS.MVC.Models.ProposalWritten;
using INZFS.MVC.ViewModels.ProposalWritten;

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
        private readonly dynamic New;
        private readonly INotifier _notifier;
        private readonly ISession _session;
        private readonly ISiteService _siteService;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly INavigation _navigation;

        public FundApplicationController(IContentManager contentManager, IContentDefinitionManager contentDefinitionManager, 
            IContentItemDisplayManager contentItemDisplayManager, IHtmlLocalizer<FundApplicationController> htmlLocalizer,
            INotifier notifier, ISession session, IShapeFactory shapeFactory, ISiteService siteService, 
            IUpdateModelAccessor updateModelAccessor, INavigation navigation)
        {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _contentItemDisplayManager = contentItemDisplayManager;
            _notifier = notifier;
            _session = session;
            _siteService = siteService;
            _updateModelAccessor = updateModelAccessor;

            H = htmlLocalizer;
            New = shapeFactory;
            _navigation = navigation;
        }

        [HttpGet]
        public async Task<IActionResult> Section(string pagename, string id)
        {
            pagename = pagename.ToLower().Trim();

            if (pagename == "application-summary")
            {
                return View("ApplicationSummary", new ApplicationSummaryModel());
            }

            // 
            if (pagename == "proposal-written-summary")
            {
                var model = await GetApplicationWrittenSummaryModel();
                return View("ProposalWrittenSummary", model);
            }
            if (pagename == "summary")
            {
                var model = await GetSummaryModel();
                return View("Summary", model);
            }

            var page = _navigation.GetPage(pagename);
            if(page == null)
            {
                return NotFound();
            }
            else
            {
                if(page is ViewPage)
                {
                    return View(((ViewPage)page).ViewName);
                }
                else
                {

                }
                return await Create(((ContentPage)page).ContentType);
            }
        }
        public async Task<IActionResult> Index(PagerParameters pagerParameters)
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
            if(records > 0)
            {
                var existingContentItem = await query.FirstOrDefaultAsync();
                return  await Edit(existingContentItem.ContentItemId, contentType);
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

        private async Task<SummaryViewModel> GetSummaryModel()
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

            return model;
        }

        private async Task<ProposalWrittenSummaryViewModel> GetApplicationWrittenSummaryModel()
        {
            var query = _session.Query<ContentItem, ContentItemIndex>();
            query = query.With<ContentItemIndex>(x => x.ContentType == "ProjectProposalDetails"
            || x.ContentType == "ProjectExperience");
            query = query.With<ContentItemIndex>(x => x.Published);
            query = query.With<ContentItemIndex>(x => x.Author == User.Identity.Name);

            var items = await query.ListAsync();
            var projectProposal = items.FirstOrDefault(item => item.ContentType == "ProjectProposalDetails");
            var projectProposalPart = projectProposal?.ContentItem.As<ProjectProposalDetailsPart>();

            var projectExperience = items.FirstOrDefault(item => item.ContentType == "ProjectExperience");
            var projectExperiencePart = projectExperience?.ContentItem.As<ProjectExperiencePart>();

            var model = new ProposalWrittenSummaryViewModel()
            {
                ProjectProposalDetailsViewModel = new ProjectProposalDetailsViewModel
                {
                    InnovationImpactSummary = projectProposalPart.InnovationImpactSummary,
                    Day = projectProposalPart.Day,
                    Month = projectProposalPart.Month,
                    Year = projectProposalPart.Year,
                },
                ProjectExperienceViewModel = new ProjectExperienceViewModel
                {
                    ExperienceSummary = projectExperiencePart.ExperienceSummary,
                }
            };

            return model;
        }
    }
}

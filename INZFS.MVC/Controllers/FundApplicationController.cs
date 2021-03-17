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
        private readonly Dictionary<string, string> parts = new Dictionary<string, string>();

        public FundApplicationController(IContentManager contentManager, IContentDefinitionManager contentDefinitionManager, IContentItemDisplayManager contentItemDisplayManager, IHtmlLocalizer<FundApplicationController> htmlLocalizer, INotifier notifier, ISession session, IShapeFactory shapeFactory, ISiteService siteService, IUpdateModelAccessor updateModelAccessor)
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
            parts.Add("proposal-summary", "ProposalSummaryPart");
            parts.Add("person-summary", "PersonPage");

        }

        public async Task<IActionResult> Section(string pagename, string id)
        {
            pagename = pagename.ToLower().Trim();
            if(pagename == "summary")
            {
                var query = _session.Query<ContentItem, ContentItemIndex>();
                query = query.With<ContentItemIndex>(x => x.ContentType == "ProposalSummaryPart" || x.ContentType == "PersonPage");
                query = query.With<ContentItemIndex>(x => x.Published);
                query = query.With<ContentItemIndex>(x => x.Author == User.Identity.Name);

                var items = await query.ListAsync();


                //var contentItem = await _contentManager.GetAsync("ProposalSummaryPart", VersionOptions.Latest);
                //var part = contentItem.ContentItem.As<ProposalSummaryPart>();
                return View("Summary");
            }
            if(parts.Keys.Contains(pagename))
            {
                return await Create(parts[pagename]);
            }
            else
            {
                return NotFound();
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
            if(records > 0)
            {
                var existingContentItem = await query.FirstOrDefaultAsync();
                //return RedirectToAction("Edit", new { contentItemId = existingContentItem.ContentItemId });
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
            // pass a dummy content to the authorization check to check for "own" variations
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

            // Set the current user as the owner to check for ownership permissions on creation
            contentItem.Owner = User.Identity.Name;

            var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, true);

            if (!ModelState.IsValid)
            {
                _session.Cancel();
                return View("Create", model);
            }

            await _contentManager.CreateAsync(contentItem, VersionOptions.Draft);

            await conditionallyPublish(contentItem);

           var pageToRedirectTo = "summary";
            if (parts.Values.Contains(contentItem.ContentType) && contentItem.ContentType.ToLower() != "PersonPage".ToLower())
            {
                pageToRedirectTo = "person-summary";
                //pageToRedirectTo = parts.FirstOrDefault(x => x.Value == contentItem.ContentType).Key;
            }
            return RedirectToAction("section", new { pagename = pageToRedirectTo });
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

                //_notifier.Success(string.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                //    ? H["Your content has been published."]
                //    : H["Your {0} has been published.", typeDefinition.DisplayName]);
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

            // The content item needs to be marked as saved in case the drivers or the handlers have
            // executed some query which would flush the saved entities inside the above UpdateEditorAsync.
            _session.Save(contentItem);

            await conditionallyPublish(contentItem);

            var pageToRedirectTo = "summary";
            if (parts.Values.Contains(contentItem.ContentType) && contentItem.ContentType.ToLower() != "PersonPage".ToLower())
            {
                pageToRedirectTo = "person-summary";
                //pageToRedirectTo = parts.FirstOrDefault(x => x.Value == contentItem.ContentType).Key;
            }
            return RedirectToAction("section", new { pagename = pageToRedirectTo });
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

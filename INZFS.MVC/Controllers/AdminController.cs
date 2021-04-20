using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Settings;
using YesSql;
using INZFS.MVC.Models;
using INZFS.MVC.Forms;
namespace INZFS.MVC.Controllers
{
    public class AdminController : Controller
    {

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

        public AdminController(IContentManager contentManager, IContentDefinitionManager contentDefinitionManager,
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
        public async Task<IActionResult> GetApplicationsSearch(string ApplicationSearch)
        {

            var query = _session.Query<ContentItem, ContentItemIndex>();
            query = query.With<ContentItemIndex>(x => x.ContentType == "CompanyDetails");
            query = query.With<ContentItemIndex>(x => x.Published);

            var items = await query.ListAsync();
            var companyLists = items.Select(x => x.As<CompanyDetailsPart>());

            if (!string.IsNullOrEmpty(ApplicationSearch))
            {
                companyLists = companyLists.Where(x => x.CompanyName.Contains(ApplicationSearch));
            }

            var companySearchList = companyLists.ToList();

            var model = new ApplicationsSummaryModel
            {
                CompanyDetails = companySearchList
            };

            return View(model);
        }


    }




}



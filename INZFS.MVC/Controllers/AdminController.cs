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
using OrchardCore.Flows.Models;
using System.Collections.Generic;


namespace INZFS.MVC.Controllers
{
    public class AdminController : Controller
    {

        private readonly IContentManager _contentManager;
        private readonly ISession _session;


        public AdminController(IContentManager contentManager, ISession session)
        {
            _contentManager = contentManager;
            _session = session;
        }


        [HttpGet]
        public async Task<IActionResult> GetApplicationsSearch(string companyName)
        {

            var applications = string.IsNullOrEmpty(companyName) ? new Dictionary<string, ContentItem>() : await GetContentItemListFromBagPart(companyName);

            var model = new FundManagerApplicationsModel
            {
                Applications = applications
            };

            return View(model);
        }

        private async Task<Dictionary<string, ContentItem>> GetContentItemListFromBagPart(string companyName)
        {
            var applicatinListResult = new Dictionary<string, ContentItem>();

            var query = _session.Query<ContentItem, ContentItemIndex>();
            query = query.With<ContentItemIndex>(index => index.ContentType == ContentTypes.INZFSApplicationContainer);
            query = query.With<ContentItemIndex>(x => x.Published);

            var applications = await query.ListAsync();

            foreach (var application in applications)
            {
                var applicationContainer = application?.ContentItem.As<BagPart>();

                var contentItem = applicationContainer.ContentItems.FirstOrDefault(item => item.ContentType == ContentTypes.CompanyDetails);
                if (contentItem != null)
                {
                    var companyDetailsPart = contentItem?.ContentItem.As<CompanyDetailsPart>();

                    if (companyDetailsPart.CompanyName.ToLower().Contains(companyName.ToLower()))
                    {
                        applicatinListResult.Add(companyDetailsPart.CompanyName, application);
                    }
                }

            }


            return applicatinListResult;
        }

        [HttpGet]
        public async Task<IActionResult> Application(string id)
        {

            var query = _session.Query<ContentItem, ContentItemIndex>();
            query = query.With<ContentItemIndex>(index => index.ContentItemId == id.Trim());
            query = query.With<ContentItemIndex>(x => x.Published);

            var application = await query.FirstOrDefaultAsync();

            var bagPart = application.ContentItem.As<BagPart>();
            var contents = bagPart.ContentItems;

            return View(contents);
        }


    }

}



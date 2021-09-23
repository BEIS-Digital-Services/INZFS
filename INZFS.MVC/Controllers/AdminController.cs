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
using System.Linq.Expressions;
using System;

namespace INZFS.MVC.Controllers
{
    public class AdminController : Controller
    {

        private readonly IContentRepository _contentRepository;

        public AdminController(IContentRepository contentRepository)
        {
            _contentRepository = contentRepository;
        }


        [HttpGet]
        public async Task<IActionResult> GetApplicationsSearch(string companyName)
        {
            return View(null);
        }
    }

}



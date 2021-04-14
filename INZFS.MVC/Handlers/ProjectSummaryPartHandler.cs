using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using INZFS.MVC.Drivers;
using INZFS.MVC.Forms;
using INZFS.MVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OrchardCore.ContentManagement.Handlers;

namespace INZFS.MVC.Handlers
{
    public class ProjectSummaryPartHandler : ContentPartHandler<ProjectSummaryPart>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
    
        public ProjectSummaryPartHandler(IHttpContextAccessor httpContextAccessor, ITempDataDictionaryFactory tempDataDictionaryFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _tempDataDictionaryFactory = tempDataDictionaryFactory;
        }
        public override Task UpdatingAsync(UpdateContentContext context, ProjectSummaryPart instance)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var tempData = _tempDataDictionaryFactory.GetTempData(httpContext);
            var uploadDetails = (UploadDetail)tempData["UploadDetail"];
            if(uploadDetails != null)
            {
                instance.FileUploadPath = uploadDetails.FileName.ToString();
                tempData.Clear();
            }
            return base.UpdatingAsync(context, instance);
        }
    }
}

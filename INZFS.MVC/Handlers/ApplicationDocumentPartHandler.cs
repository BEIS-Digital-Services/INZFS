using INZFS.MVC.Models;
using INZFS.MVC.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OrchardCore.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Handlers
{
    public class ApplicationDocumentPartHandler : ContentPartHandler<ApplicationDocumentPart>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;

        public ApplicationDocumentPartHandler(IHttpContextAccessor httpContextAccessor, ITempDataDictionaryFactory tempDataDictionaryFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _tempDataDictionaryFactory = tempDataDictionaryFactory;
        }
        public override Task UpdatingAsync(UpdateContentContext context, ApplicationDocumentPart instance)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var tempData = _tempDataDictionaryFactory.GetTempData(httpContext);
            var uploadDetails = (UploadDetail)tempData["UploadDetail"];
            if (uploadDetails != null)
            {
                switch (uploadDetails.ContentItemProperty)
                {
                    case "ProjectPlan":
                        instance.ProjectPlan = uploadDetails.FileName;
                        break;
                    case "ExperienceAndSkills":
                        instance.ExperienceAndSkills = uploadDetails.FileName;
                        break;
                }
                tempData.Clear();
            }
            return base.UpdatingAsync(context, instance);
        }
    }
}

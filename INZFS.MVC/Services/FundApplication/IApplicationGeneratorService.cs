using INZFS.MVC.Models.DynamicForm;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Services.FundApplication
{
    public interface IApplicationGeneratorService
    {
        public Page GetCurrentPage(string pageName);
        public Task<ApplicationContent> GetApplicationConent(string userId);
        public Field GetField(string fieldName, string userId);

        public Section GetCurrenSection(string pageName);
        public Task<IActionResult> GetSection(string pagename, string id);
        public ApplicationOverviewContent GetApplicationOverviewContent(ApplicationContent content);

    }
}

using INZFS.MVC.Models.DynamicForm;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Services.FundApplication
{
    public interface IPersistenceService
    {
        public Page GetCurrentPage(string pageName);
        public void CheckValidityOfFile(IFormFile file, Page currentPage);
        public void CheckFieldStatus(string pageName, string submitAction, BaseModel model, string userId);
    }
}

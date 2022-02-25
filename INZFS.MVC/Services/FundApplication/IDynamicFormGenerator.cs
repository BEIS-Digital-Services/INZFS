using INZFS.MVC.Models.DynamicForm;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Services.FundApplication
{
    public interface IDynamicFormGenerator
    {
        public BaseModel PopulateModel(Page currentPage, BaseModel currentModel, Field field = null);
        public ViewResult PopulateViewModel(Page currentPage, BaseModel currentModel, Field field = null);
        public ViewResult GetViewModel(Page currentPage, Field field);
        public void SetPageTitle(string title);
    }
}

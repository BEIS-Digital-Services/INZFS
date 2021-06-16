using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models.DynamicForm
{
    public class BaseModel 
    {

        public string PageName { get; set; }
        public string Question { get; set; }
        public string ErrorMessage { get; set; }
        public bool? Mandatory { get; set; } = true;
        public string Section { get; set; }
        public string AccordianReference { get; set; }

    }



}

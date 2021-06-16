using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models.DynamicForm
{
    public class BasePart : ContentPart
    {

        public string Name { get; set; }
        public string Question { get; set; }
        public string ErrorMessage { get; set; }
        public string FieldType { get; set; }
        public bool? Mandetory { get; set; }

 

    }

}

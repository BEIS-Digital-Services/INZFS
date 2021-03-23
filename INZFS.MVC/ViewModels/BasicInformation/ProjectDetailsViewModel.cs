using INZFS.MVC.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.ViewModels
{
    public class ProjectDetailsViewModel : ProjectDetailsPart
    {
        [BindNever]
        public ProjectDetailsPart ProjectDetailsPart { get; set; }
    }
}

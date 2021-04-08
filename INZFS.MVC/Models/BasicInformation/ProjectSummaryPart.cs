using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models
{
    public class ProjectSummaryPart : ContentPart
    {
        [Required(ErrorMessage = "Enter the project name")]
        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }

        [Required(ErrorMessage = "Enter estimated start day")]
        public int? Day { get; set; }
        [Required(ErrorMessage = "Enter estimated start month")]
        public int? Month { get; set; }
        [Required(ErrorMessage = "Enter estimated start year")]
        public int? Year { get; set; }
        public string fileUploadPath { get; set; }

    }
}

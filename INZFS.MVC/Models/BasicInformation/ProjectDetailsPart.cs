using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models
{
    public class ProjectDetailsPart : ContentPart
    {
        [Required(ErrorMessage = "Enter brief summary of the project")]
        [Display(Name = "Please give a brief summary description of the project")]
        [MaxLength(200, ErrorMessage = "Exceeded Limit of characters")]
        public string Summary { get; set; }

        [Required(ErrorMessage = "Please Select Yes or No")]
        [Display(Name = "Will the BEIS project cost end by 2024?")]
        public int? Timing { get; set; }
    }
}

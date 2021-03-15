using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models
{
    public class ProposalSummaryPart : ContentPart
    {
        [Required(ErrorMessage = "Enter the project name")]
        [Display(Name ="Project Name")]
        public string ProjectName { get; set; }

        [Required(ErrorMessage = "Enter estimated start Date")]
        [Display(Name = "Estimated Start Date")]
        public DateTime? EstimatedStartDateUtc { get; set; }

        [Display(Name = "Project Duration (months)")]
        [Required(ErrorMessage = "Enter duration of the project")]
        public int? ProjectDuration { get; set; }

        [Required(ErrorMessage = "Enter brief summary of the project")]
        [Display(Name = "Please give a brief summary description of the project")]
        public string Summary { get; set; }

    }
}

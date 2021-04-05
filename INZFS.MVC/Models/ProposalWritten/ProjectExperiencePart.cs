using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models.ProposalWritten
{
    public class ProjectExperiencePart : ContentPart
    {
        [Required(ErrorMessage = "Enter brief summary of experience")]
        [Display(Name = "Please summarise the company's relevant experience in delivering projects?")]
        [MaxLength(200, ErrorMessage = "Exceeded Limit of characters")]
        public string ExperienceSummary { get; set; }
    }
}

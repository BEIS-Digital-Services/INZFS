using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models.ProposalWritten
{
    public class ProjectProposalDetailsPart : ContentPart
    {
        [Required(ErrorMessage = "Enter brief summary")]
        [Display(Name = "How will the innovation impact carbon targets?")]
        [MaxLength(200, ErrorMessage = "Exceeded Limit of characters")]
        public string InnovationImpactSummary { get; set; }

        [Required(ErrorMessage = "Enter estimated project end day")]
        public int? Day { get; set; }
        [Required(ErrorMessage = "Enter estimated project end month")]
        public int? Month { get; set; }
        [Required(ErrorMessage = "Enter estimated project end year")]
        public int? Year { get; set; }
    }
}

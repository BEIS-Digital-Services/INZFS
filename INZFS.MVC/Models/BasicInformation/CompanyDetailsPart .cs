using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models
{
    public class CompanyDetailsPart : ContentPart
    {
        [Required(ErrorMessage = "Please enter Comapany Name")]
        [Display(Name = "Company Name")]
        [MaxLength(40, ErrorMessage = "Exceeded Limit of characters")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Please enter Company Number")]
        [Display(Name = "Company Number")]
        public int? CompanyNumber { get; set; }
    }


}
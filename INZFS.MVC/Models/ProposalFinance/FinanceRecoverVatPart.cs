using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models.ProposalFinance
{
    public class FinanceRecoverVatPart : ContentPart
    {
        [Required(ErrorMessage = "Select an option")]
        [Display(Name = "Is the organisation able to recover VAT?")]
        public bool AbleToRecover { get; set; }

    }

}

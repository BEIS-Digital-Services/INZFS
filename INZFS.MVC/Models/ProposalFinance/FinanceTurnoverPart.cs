using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models.ProposalFinance
{
    public class FinanceTurnoverPart : ContentPart
    {
        [Required(ErrorMessage = "Enter the turnover amount")]
        [Display(Name = "Turnover Amount  (in most recent annual accounts)")]
        public decimal? TurnoverAmount { get; set; }

        [Required(ErrorMessage = "Enter turnover day")]
        public int? Day { get; set; }
        [Required(ErrorMessage = "Enter turnover month")]
        public int? Month { get; set; }
        [Required(ErrorMessage = "Enter turnover year")]
        public int? Year { get; set; }
    }

}

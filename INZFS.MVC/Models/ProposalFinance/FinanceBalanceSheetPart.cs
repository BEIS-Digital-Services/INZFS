using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models.ProposalFinance
{
    public class FinanceBalanceSheetPart : ContentPart
    {
        [Required(ErrorMessage = "Enter the balance sheet total")]
        [Display(Name = "Balance sheet total")]
        public decimal? BalanceSheetTotal { get; set; }

        [Required(ErrorMessage = "Enter balance sheet day")]
        public int? Day { get; set; }
        [Required(ErrorMessage = "Enter balance sheet month")]
        public int? Month { get; set; }
        [Required(ErrorMessage = "Enter balance sheet year")]
        public int? Year { get; set; }
    }

}

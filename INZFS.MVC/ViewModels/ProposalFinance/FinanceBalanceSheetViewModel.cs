using INZFS.MVC.Models.ProposalFinance;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.ViewModels.ProposalFinance
{
    public class FinanceBalanceSheetViewModel : FinanceBalanceSheetPart
    {
        public string BalanceSheetUtc { get; set; }
        public decimal BalanceSheetTotal { get; set; }

        [BindNever]
        public FinanceBalanceSheetPart FinanceBalanceSheetPart { get; set; }
    }
}

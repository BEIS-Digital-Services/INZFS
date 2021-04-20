using INZFS.MVC.ViewModels.ProposalFinance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models
{
    public class ProposalFinanceSummaryViewModel
    {
        public FinanceTurnoverViewModel FinanceTurnoverViewModel { get; set; }
        public FinanceBalanceSheetViewModel FinanceBalanceSheetViewModel { get; set; }
        public FinanceRecoverVatViewModel FinanceRecoverVatViewModel { get; set; }
        public FinanceBarriersViewModel FinanceBarriersViewModel { get; set; }
    }
}

using INZFS.MVC.Models.ProposalFinance;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.ViewModels.ProposalFinance
{
    public class FinanceRecoverVatViewModel : FinanceRecoverVatPart
    {
        public bool AbleToRecover { get; set; }

        [BindNever]
        public FinanceRecoverVatPart FinanceRecoverVatPart { get; set; }
    }
}

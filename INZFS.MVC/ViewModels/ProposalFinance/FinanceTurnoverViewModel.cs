using INZFS.MVC.Models.ProposalFinance;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.ViewModels.ProposalFinance
{
    public class FinanceTurnoverViewModel : FinanceTurnoverPart
    {
        public string TurnoverUtc { get; set; }

        [BindNever]
        public FinanceTurnoverPart FinanceTurnoverPart { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.ViewModels
{
    public class SummaryViewModel
    {

        public CompanyDetailsViewModel CompanyDetailsViewModel { get; set; }
        public OrgFundingViewModel OrgFundingViewModel { get; set; }
        public ProjectDetailsViewModel ProjectDetailsViewModel { get; set; }
        public ProjectSummaryViewModel ProjectSummaryViewModel { get; set; }
    }
}
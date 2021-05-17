using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models
{
    public class ApplicationSummaryModel
    {
        public int TotalSections { get; set; }
        public int TotalCompletedSections { get; set; }
        public Sections CompletedSections { get; set; }
    }

    [Flags]
    public enum Sections
    {
        None = 0,
        ProjectSummary = 1 << 0,
        ProjectDetails = 1 << 1,
        Funding = 1 << 2,
        ProjectProposalDetails = 1 << 3,
        ProjectPlanUpload = 1 << 4,
        ProjectExperience = 1 << 5,
        ProjectExperienceSkillsUpload = 1 << 6,
        FinanceTurnover = 1 << 7,
        FinanceBalanceSheet = 1 << 8,
        FinanceRecoverVat = 1 << 9,
        FinanceBarriers = 1 << 10,
        CompanyDetails = 1 << 11,

    }
}

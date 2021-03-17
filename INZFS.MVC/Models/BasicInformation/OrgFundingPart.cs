using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models
{
    public class OrgFundingPart : ContentPart
    {
        [Required(ErrorMessage = "Select Items That Apply")]
        [Display(Name = "How is the organisation funded? (Choose all that apply)")]
        public OrgFundingOptions Funding { get; set; }

    }

    public enum OrgFundingOptions
    {
        NoFunding,
        Founders,
        FriendsAndFamily,
        PublicSectorGrants,
        AngelInvestment,
        VentureCapital,
        PriavateEquity,
        SotckMarketFlotation
    }
}

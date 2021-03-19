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
        public bool NoFunding { get; set; } = false;
        public bool Funders { get; set; } = false;
        public bool FriendsAndFamily { get; set; } = false;
        public bool PublicSectorGrants { get; set; } = false;
        public bool AngelInvestment { get; set; } = false;
        public bool VentureCapital { get; set; } = false;
        public bool PrivateEquity { get; set; } = false;
        public bool StockMarketFlotation { get; set; } = false;

    }

   
}

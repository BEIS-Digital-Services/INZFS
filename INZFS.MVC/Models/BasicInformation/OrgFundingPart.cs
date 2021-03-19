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
        public bool NoFunding { get; set; }
        public bool Funders { get; set; }
        public bool FriendsAndFamily { get; set; }
        public bool PublicSectorGrants { get; set; }
        public bool AngelInvestment { get; set; }
        public bool VentureCapital { get; set; }
        public bool PrivateEquity { get; set; }
        public bool StockMarketFlotation { get; set; }

    }

   
}

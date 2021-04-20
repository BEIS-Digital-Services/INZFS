using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models
{
    public class FundManagerApplicationsModel
    {
        public IList<CompanyDetailsPart> CompanyDetails { get; set; }

        public string ApplicationSearch { get; set; }
    }
}
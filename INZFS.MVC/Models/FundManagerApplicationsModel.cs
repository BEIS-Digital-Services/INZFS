using OrchardCore.ContentManagement;
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

        public Dictionary<string, ContentItem> Applications { get; set; }

        public string CompanyName { get; set; }
    }
}
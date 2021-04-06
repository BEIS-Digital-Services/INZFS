using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models
{
    public class ApplicationSummaryModel
    {
        public bool IsComplete { get; set; }
        public int TotalSections { get; set; }
        public int CompletedSections { get; set; }
    }
}

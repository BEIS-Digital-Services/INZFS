using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models.DynamicForm
{
    public class ApplicationOverviewModel
    {
        public string SectionTag { get; set; }
        public string Url { get; set; }
        public string Status { get; set; }
    }

    public class ApplicationOverviewContent
    {
        public ApplicationOverviewContent()
        {
            Sections = new List<ApplicationOverviewModel>();
        }
        public List<ApplicationOverviewModel> Sections { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalSectionsCompleted { get; set; }
    }
}

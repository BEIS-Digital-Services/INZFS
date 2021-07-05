using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models.DynamicForm
{
    public class SectionModel
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Status { get; set; }
    }

    public class SectionContent
    {
        public SectionContent()
        {
            Sections = new List<SectionModel>();
        }
        public List<SectionModel> Sections { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models.Report
{
    public class ReportContentModel
    {
        public string ApplicationNumber { get; set; }
        public byte[] FileContents { get; set; }
    }
}

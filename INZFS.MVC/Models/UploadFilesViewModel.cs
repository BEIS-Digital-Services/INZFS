using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models
{
    public class UploadFilesViewModel
    {
        public IList<ScanResult> Results { get; set; }
    }

    public class ScanResult
    {
        public string FileName { get; set; }
        public string Result { get; set; }
        public string Message { get; set; }
        public string RawResult { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Settings
{
    public class ApplicationOption
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool VirusScanningEnabled { get; set; }
    }
}

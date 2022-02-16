using System;

namespace INZFS.MVC.Settings
{
    public class ApplicationOption
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool VirusScanningEnabled { get; set; }
        public string CloudmersiveApiKey { get; set; }
    }
}

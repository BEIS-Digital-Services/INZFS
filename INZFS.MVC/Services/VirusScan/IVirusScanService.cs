using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Services.VirusScan
{
    public interface IVirusScanService
    {
        public string ScanFile(IFormFile file);
    }
}

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INZFS.Web.ViewModels
{
    public class EligibilityCheckerViewModel
    {
        [TempData]
        public bool Question1 { get; set; }
        [TempData]
        public bool Question2 { get; set; }
        [TempData]
        public bool Question3 { get; set; }
        [TempData]
        public bool Question4 { get; set; }
        [TempData]
        public bool Question5 { get; set; }
        [TempData]
        public bool Question6 { get; set; }
        [TempData]
        public string RedirectUrl { get; set; }

 
    }
}

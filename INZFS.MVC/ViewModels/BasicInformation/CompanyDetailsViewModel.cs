using INZFS.MVC.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.ViewModels
{
    public class CompanyDetailsViewModel : CompanyDetailsPart
    {

        [BindNever]
        public CompanyDetailsPart CompanyDetailsPart { get; set; }
    }
}
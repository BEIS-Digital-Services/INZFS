using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models
{
    public class ApplicationStatusPart : ContentPart
    {
        public Status ApplicationSatatus { get; set; }
    }

  
    public enum Status
    {
        Approved = 0 ,
        Rejected = 1
    }
}
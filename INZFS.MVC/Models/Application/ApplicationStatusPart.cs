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
        public ApplictionStatus ApplictionStatus { get; set; }

    }

    public enum ApplictionStatus
    {
        Successful,
        Unsuccesful 
    }

}
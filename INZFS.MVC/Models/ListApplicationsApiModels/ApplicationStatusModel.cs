using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models.ListApplicationsApiModels
{
    public class ApplicationStatusModel
    {
        public int DocumentId { get; set; }
        public string ApplicationId { get; set; }
        public string ApplicantName { get; set; }
        public string CompanyName { get; set; }
        public ApplicationStatus ApplicationStatus { get; set; }
    }
}

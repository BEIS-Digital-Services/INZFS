using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models
{
    public class ApplicationSentModel
    {
        public string ApplicationNumber { get; set; }
        public string ApplicationStatus { get; set; }
        public DateTime SubmittedDate { get; set; }
        public ApplicationStatus Status { get; set; }
        public ApplicationOutcomeStatusType OutcomeStatus { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Extensions
{
    public static class ApplicationStatusExtension
    {
        public static string ToStatusString(this ApplicationStatus status)
        {
            switch (status)
            {
                case ApplicationStatus.InProgress:
                    return "In Progress";
                case ApplicationStatus.Submitted:
                    return "Submitted";
                case ApplicationStatus.Withdrawn:
                    return "Withdrawn";
                case ApplicationStatus.NotSubmitted:
                    return "Not Submitted";
                default:
                    throw new Exception("Invalid status");
            }
        }
        
        public static string ToOutcomeStatusString(this ApplicationOutcomeStatusType outcome, ApplicationStatus status)
        {
            if (outcome == ApplicationOutcomeStatusType.None)
            {
                return status.ToStatusString();
            }

            switch (outcome)
            {
                case ApplicationOutcomeStatusType.InReview:
                    return "In Review";

                case ApplicationOutcomeStatusType.InModeration:
                    return "In Independent Assessment";

                case ApplicationOutcomeStatusType.ModerationComplete:
                    return "Assessment completed";

                case ApplicationOutcomeStatusType.Successful:
                    return "Successful";

                case ApplicationOutcomeStatusType.Unsuccessful:
                    return "Unsuccessful";

                default:
                    throw new Exception("Invalid outcome status");
            }
        }
    }

    
}

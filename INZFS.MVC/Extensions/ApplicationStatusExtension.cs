using INZFS.MVC.Models.Application;
using System;

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
                case ApplicationStatus.Assessment:
                    return "Submitted";
                case ApplicationStatus.IndependentAssessment:
                    return "Independent Assessment";
                case ApplicationStatus.Successful:
                    return "Successful";
                case ApplicationStatus.Unsuccessful:
                    return "Unsuccessful";
                case ApplicationStatus.Withdrawn:
                    return "Withdrawn";
                case ApplicationStatus.NotSubmitted:
                    return "Not Submitted";
                default:
                    throw new Exception("Invalid status");
            }
        }
    }

    
}

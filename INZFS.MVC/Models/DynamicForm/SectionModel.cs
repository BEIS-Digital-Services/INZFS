using INZFS.MVC.Models.Application;
using System;
using System.Collections.Generic;
using System.Linq;

namespace INZFS.MVC.Models.DynamicForm
{
    public enum SectionStatus
    {
        NotStarted,
        InProgress,
        Completed,
        NotApplicable
    }
    public class ApplicationOverviewContent
    {
        public ApplicationOverviewContent()
        {
            //Sections = new List<SectionContent>();
            Sections = new List<ApplicationOverviewModel>();
            HasErrors = false;
        }
        public List<ApplicationOverviewModel> Sections { get; set; }
        //public List<SectionContent> Sections { get; set; }
        public int TotalSections { get; set; }
        public int TotalSectionsCompleted { get; set; }
        public string ApplicationNumber { get; set; }
        public bool HasErrors { get; set; }
    }

    public class ApplicationOverviewModel : SectionModel
    {
    }

    public class SectionModel
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string SectionTag { get; set; }
        public bool HideFromSummary { get; set; }
        public FieldStatus SectionStatus { get; set; }
        public string GetStatusString()
        {
            switch (SectionStatus)
            {
                case FieldStatus.NotStarted:
                    return "Not started";
                case FieldStatus.InProgress:
                    return "In progress";
                case FieldStatus.Completed:
                    return "Completed";
                case FieldStatus.NotApplicable:
                    return "Not applicable";
                default:
                    throw new Exception("Invalid status");
            }
        }
    }

    public class SectionContent
    {
        public SectionContent()
        {
            Sections = new List<SectionModel>();
        }
        public string Title { get; set; }
        public string OverviewTitle { get; set; }
        public string Url { get; set; }
        public string ReturnUrl { get; set; }
        public List<SectionModel> Sections { get; set; }
        public FieldStatus OverallStatus
        {
            get {
                if (Sections.All(section => (section.SectionStatus == FieldStatus.Completed || section.SectionStatus == FieldStatus.NotApplicable)))
                {
                    return FieldStatus.Completed;
                }
                if (Sections.Any(section => section.SectionStatus == FieldStatus.InProgress || section.SectionStatus == FieldStatus.Completed))
                {
                    return FieldStatus.InProgress;
                }
                if (Sections.All(section => section.SectionStatus == FieldStatus.NotStarted))
                {
                    return FieldStatus.NotStarted;
                }

                return FieldStatus.NotApplicable;
            }
        }
        public int TotalQuestions { get; set; }
        public int TotalQuestionsCompleted { get; set; }
        public string ApplicationNumber { get; set; }
        public bool HasErrors { get; set; }
    }
}


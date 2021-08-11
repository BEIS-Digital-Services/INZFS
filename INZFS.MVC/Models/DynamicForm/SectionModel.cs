using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
        public List<ApplicationOverviewModel> Sections { get; set; }
        //public List<SectionContent> Sections { get; set; }
        public int TotalSections { get; set; }
        public int TotalSectionsCompleted { get; set; }
    }

    public class ApplicationOverviewModel : SectionModel
    {
    }

    public class SectionModel
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string SectionTag { get; set; }
        public SectionStatus SectionStatus { get; set; }
        public string GetStatusString()
        {
            switch (SectionStatus)
            {
                case SectionStatus.NotStarted:
                    return "Not Started";
                case SectionStatus.InProgress:
                    return "In Progress";
                case SectionStatus.Completed:
                    return "Completed";
                case SectionStatus.NotApplicable:
                    return "Not Applicable";
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
        public SectionStatus OverallStatus
        {
            get {
                if (Sections.All(section => section.SectionStatus == SectionStatus.Completed))
                {
                    return SectionStatus.Completed;
                }
                if (Sections.Any(section => section.SectionStatus == SectionStatus.InProgress || section.SectionStatus == SectionStatus.Completed))
                {
                    return SectionStatus.InProgress;
                }
                if (Sections.All(section => section.SectionStatus == SectionStatus.NotStarted))
                {
                    return SectionStatus.NotStarted;
                }

                return SectionStatus.NotApplicable;
            }
        }
        public int TotalQuestions { get; set; }
        public int TotalQuestionsCompleted { get; set; }
    }
}


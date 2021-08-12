using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models.DynamicForm
{
    public abstract class BaseModel : IValidatableObject
    {
        public string Kind { get; set; }
        public int QuestionNumber { get; set; } = 1;
        public int TotalQuestions { get; set; }
        public bool DisplayQuestionCounter { get; set; } = false;
        public string PageName { get; set; }
        public string PreviousPageName { get; set; }
        public string FieldName { get; set; }
        public string Question { get; set; }
        public string TitleQuestion { get; set; }
        public string Description { get; set; }
        public string ErrorMessage { get; set; }
        public bool? Mandatory { get; set; } = true;
        public string Section { get; set; }
        public string AccordianReference { get; set; }
        public string DataInput { get; set; }

        public bool ShowMarkAsComplete { get; set; }
        public bool MarkAsComplete { get; set; }
        public string Hint { get; set; }
        public int? MaxLength { get; set; }
        public string? NextPageName { get; set; }
        public bool ShowSaveProgessButton { get; set; }
        public string ReturnToSummaryPageLinkText { get; set; }
        public string ContinueButtonText { get; set; }
        public string SectionUrl { get; set; }
        public Section SectionInfo { get; set; }
        public string FileToDownload { get; set; }
        public string UploadText { get; set; }
        public List<Action> Actions { get; set; }
        public MaxLengthValidationType MaxLengthValidationType { get; set; }
        protected ApplicationDefinition ApplicationDefinition { get; set; }
        protected Page CurrentPage { get; set; }

        public virtual string GetData()
        {
            return DataInput;
        }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            ApplicationDefinition = (ApplicationDefinition)validationContext.GetService(typeof(ApplicationDefinition));
            var page = ApplicationDefinition.Application.AllPages.FirstOrDefault(p => p.Name.ToLower().Equals(PageName));
            CurrentPage = page;
            ErrorMessage = page.ErrorMessage;
            Mandatory = page.Mandatory;
            Hint = page.Hint;
            ShowMarkAsComplete = page.ShowMarkComplete;
            return ExtendedValidation(validationContext);
        }

        protected abstract IEnumerable<ValidationResult> ExtendedValidation(ValidationContext validationContext);
        
    }



}

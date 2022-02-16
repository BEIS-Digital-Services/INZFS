using INZFS.MVC.Models.Application;
using INZFS.MVC.Validators;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Action = INZFS.MVC.Models.Application.Action;

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
        public string SectionTitle { get; set; }
        public string Question { get; set; }
        public string FriendlyFieldName { get; set; }
        public string Description { get; set; }
        public string ErrorMessage { get; set; }
        public bool? Mandatory { get; set; } = true;
        public string Section { get; set; }
        public TextType TextType { get; set; }
        public YesNoType YesNoInput { get; set; }
        public UploadedFile UploadedFile { get; set; }

        public string AccordianReference { get; set; }
        public string DataInput { get; set; }
        public string OtherOption { get; set; }
        public List<string> SelectedOptions { get; set; }
        public List<string> UserOptionsSelected { get; set; }
        public bool HasOtherOption { get; set; }
        public bool ShowMarkAsComplete { get; set; }
        public bool MarkAsComplete { get; set; }
        public bool ResultAcknowledged { get; set; }
        public string Hint { get; set; }
        public int? MaxLength { get; set; }
        public string NextPageName { get; set; }
        public string ReturnPageName { get; set; }
        public string ReturnToSummaryPageLinkText { get; set; }
        public string ContinueButtonText { get; set; }
        public string SectionUrl { get; set; }
        public Section SectionInfo { get; set; }
        public string FileToDownload { get; set; }
        public List<Action> Actions { get; set; }
        public MaxLengthValidationType MaxLengthValidationType { get; set; }
        protected ApplicationDefinition ApplicationDefinition { get; set; }
        public Page CurrentPage { get; set; }
        public PreviousPage PreviousPage { get; set; }
        public FieldStatus? FieldStatus { get; set; }
        public FieldType FieldType { get; set; }
        public string AcceptableFileExtensions { get; set; }

        public GridDisplayType GridDisplayType { get; set; }
        public bool HideQuestionCounter { get; set; }
        public bool HideBreadCrumbs { get; set; }
        public virtual string GetData()
        {
            return DataInput;
        }

        public virtual string GetOtherSelected()
        {
            return OtherOption;
        }

        public virtual List<string> GetSelectedByUser()
        {
            return UserOptionsSelected;
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


            var actionErrors = ValidateActions(page);
            if (actionErrors.Any())
            {
                return actionErrors;
            }

            var errors = ExtendedValidation(validationContext);
            if(!errors.Any())
            {
                if (!string.IsNullOrEmpty(page.CustomValidator))
                {
                    var factory = (ICustomerValidatorFactory)validationContext.GetService(typeof(ICustomerValidatorFactory));
                    var customValidator = factory.Get(page.CustomValidator);
                    return customValidator.Validate(this, page);
                }
            }


            return errors;
        }

        protected abstract IEnumerable<ValidationResult> ExtendedValidation(ValidationContext validationContext);

        private IEnumerable<ValidationResult> ValidateActions(Page page) 
        {
            if (page.Actions != null && page.Actions.Count > 0)
            {
                var userInput = GetData();
                if (string.IsNullOrEmpty(userInput))
                {
                    yield return new ValidationResult($" Choose mandatory field {CurrentPage.FriendlyFieldName.ToLower()} before continuing", new[] { nameof(DataInput) });
                }
            }

        }
    }
}

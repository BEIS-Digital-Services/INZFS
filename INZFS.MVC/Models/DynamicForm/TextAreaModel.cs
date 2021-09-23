using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models.DynamicForm
{
    public class TextAreaModel : BaseModel
    {
        protected override IEnumerable<ValidationResult> ExtendedValidation(ValidationContext validationContext)
        {
            if (Mandatory == true && MarkAsComplete)
            {
                if (string.IsNullOrEmpty(DataInput))
                {
                    yield return new ValidationResult($"Enter {CurrentPage.FriendlyFieldName.ToLower()} before marking as complete", new[] { nameof(DataInput) });
                }
            }
            if (!string.IsNullOrEmpty(DataInput))
            {
                if(CurrentPage.MaxLengthValidationType == MaxLengthValidationType.Character)
                {
                    if (DataInput.Length > MaxLength)
                    {
                        yield return new ValidationResult($"{CurrentPage.FriendlyFieldName} must be {CurrentPage.MaxLength} characters or fewer", new[] { nameof(DataInput) });
                    }
                }
                else
                {
                    var numberOfWords = DataInput.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
                    if (numberOfWords > CurrentPage.MaxLength)
                    {
                        yield return new ValidationResult($"{CurrentPage.FriendlyFieldName} must be {CurrentPage.MaxLength} words or fewer", new[] { nameof(DataInput) });
                    }
                }
                
                
            }
        }
    }

}

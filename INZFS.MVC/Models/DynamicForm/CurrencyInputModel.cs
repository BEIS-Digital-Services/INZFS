using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models.DynamicForm
{
    public class CurrencyInputModel : BaseModel
    {
        protected override IEnumerable<ValidationResult> ExtendedValidation(ValidationContext validationContext)
        {
            if (Mandatory == true)
            {
                if (string.IsNullOrEmpty(DataInput))
                {
                    yield return new ValidationResult(ErrorMessage, new[] { nameof(DataInput) });
                }
                else
                {
                    if (DataInput.Length > CurrentPage.MaxLength)
                    {
                        yield return new ValidationResult($"{CurrentPage.FriendlyFieldName} must be {CurrentPage.MaxLength} characters or fewer", new[] { nameof(DataInput) });
                    }
                    else
                    {
                        var currencyValue = System.Convert.ToInt64(DataInput);

                        if (CurrentPage.FieldName.Equals("parent-recent-turnover") && currencyValue > 1500000)
                        {
                            yield return new ValidationResult($"Your parent company's {CurrentPage.FriendlyFieldName} cannot be greater than £1,500,000", new[] { nameof(DataInput) });
                        }
                    }                    
                }
            }
        }
    }

}

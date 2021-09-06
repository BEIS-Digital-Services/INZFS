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
            if (Mandatory == true  && MarkAsComplete)
            {
                if (string.IsNullOrEmpty(DataInput))
                {
                    yield return new ValidationResult($"Enter {CurrentPage.FriendlyFieldName.ToLower()} before marking as complete", new[] { nameof(DataInput) });
                }

            }
            if (!string.IsNullOrEmpty(DataInput))
            {
                if (DataInput.Length > CurrentPage.MaxLength)
                {
                    yield return new ValidationResult($"{CurrentPage.FriendlyFieldName} must be {CurrentPage.MaxLength} characters or fewer", new[] { nameof(DataInput) });
                }
                var numberOnly = DataInput.Replace(",", "");

                double currencyValue = 0.0;
                if (!double.TryParse(numberOnly, out currencyValue))
                {
                    yield return new ValidationResult($"{CurrentPage.FriendlyFieldName} must only include numbers, commas and full-stops", new[] { nameof(DataInput) });
                }
            }
        }
    }

}

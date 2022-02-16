using INZFS.MVC.Models.Application;
using INZFS.MVC.Models.DynamicForm;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace INZFS.MVC.Validators
{
    public class TurnOverValidator : ICustomValidator
    {
        public IEnumerable<ValidationResult> Validate(BaseModel model, Page currentPage)
        {
            var dataInput = model.GetData();
            if (!string.IsNullOrEmpty(dataInput))
            {
                var numberOnly = dataInput.Replace(",", "");
                double currencyValue = 0.0;
                if (double.TryParse(numberOnly, out currencyValue))
                {
                    if (currencyValue > 1500000)
                    {
                        yield return new ValidationResult($"Company's {currentPage.FriendlyFieldName} cannot be greater than £1,500,000", new[] { nameof(dataInput) });
                    }
                }
            }
        }
    }
}

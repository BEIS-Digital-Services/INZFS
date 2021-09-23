using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Validators
{
    public class TurnOverValidator : ICustomValidator
    {
        public IEnumerable<ValidationResult> Validate(string dataInput, string friendlyFriendlyFieldName)
        {

            if (!string.IsNullOrEmpty(dataInput))
            {
                var numberOnly = dataInput.Replace(",", "");
                double currencyValue = 0.0;
                if (double.TryParse(numberOnly, out currencyValue))
                {
                    if (currencyValue > 1500000)
                    {
                        yield return new ValidationResult($"Company's {friendlyFriendlyFieldName} cannot be greater than £1,500,000", new[] { nameof(dataInput) });
                    }
                }
            }
        }
    }
}

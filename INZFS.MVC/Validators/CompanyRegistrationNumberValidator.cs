using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Validators
{
    public class CompanyRegistrationNumberValidator : ICustomValidator
    {
        public IEnumerable<ValidationResult> Validate(string dataInput, string friendlyFriendlyFieldName)
        {
            if (!string.IsNullOrEmpty(dataInput))
            {
                if (dataInput.Length < 7 || dataInput.Length > 9)
                {
                    yield return new ValidationResult($"{friendlyFriendlyFieldName} must be between 7 and 9 characters", new[] { nameof(dataInput) });
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Validators
{
    public class PastOrPresentDateValidator : ICustomValidator
    {
        public IEnumerable<ValidationResult> Validate(string dataInput, string friendlyFriendlyFieldName)
        {
            DateTime userDateInput;
            DateTime currentDate = DateTime.UtcNow.Date;
            if (DateTime.TryParseExact(dataInput, "d/M/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out userDateInput))
            {
                if (userDateInput > currentDate)
                {
                    yield return new ValidationResult($"{friendlyFriendlyFieldName} must be in the past", new[] { "DateUtc" });
                }
            }
        }
    }
}

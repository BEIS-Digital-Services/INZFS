using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Validators
{
    public class ProposalSummaryStartDateValidator : ICustomValidator
    {
        public IEnumerable<ValidationResult> Validate(string dataInput, string friendlyFriendlyFieldName)
        {
            DateTime startDate;
            if (DateTime.TryParseExact(dataInput, "d/M/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out startDate))
            {
                if(startDate > new DateTime(2023, 3, 31))
                {
                    yield return new ValidationResult($"{friendlyFriendlyFieldName} must be the same as or before 31st March 2023", new[] { "DateUtc" });
                }
            }
        }
    }
}

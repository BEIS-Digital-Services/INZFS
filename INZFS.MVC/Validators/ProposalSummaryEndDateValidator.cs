using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Validators
{
    public class ProposalSummaryEndDateValidator : ICustomValidator
    {
        public IEnumerable<ValidationResult> Validate(string dataInput, string friendlyFriendlyFieldName)
        {
            DateTime endDate;
            if (DateTime.TryParseExact(dataInput, "d/M/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out endDate))
            {
                if (endDate > new DateTime(2025, 3, 31))
                {
                    yield return new ValidationResult($"{friendlyFriendlyFieldName} must be the same as or before 31st March 2025", new[] { "DateUtc" });
                }
                if (endDate < new DateTime(2022, 3, 31))
                {
                    yield return new ValidationResult($"{friendlyFriendlyFieldName} must be after the 31st March 2022", new[] { "DateUtc" });
                }
            }
        }
    }
}

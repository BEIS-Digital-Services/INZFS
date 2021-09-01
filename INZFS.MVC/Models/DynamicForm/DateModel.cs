using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models.DynamicForm
{
    public class DateModel : BaseModel, IValidatableObject
    {
        public string DateUtc { get; set; }
        public int? Day { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        protected override IEnumerable<ValidationResult> ExtendedValidation(ValidationContext validationContext)
        {

            DateUtc = $"{Day}/{Month}/{Year}";


            if (Mandatory == true && MarkAsComplete)
            {
                var clock = DateTime.UtcNow;
                if (string.IsNullOrEmpty(DateUtc))
                {
                    yield return new ValidationResult($"Enter {CurrentPage.FriendlyFieldName.ToLower()}", new[] { nameof(DateUtc) });
                }

            }

            if (!string.IsNullOrEmpty(DateUtc) && Mandatory == true)
            {
                DateTime userInputDate;
                string dateToValidate = $"{Day}/{Month}/{Year}";
                if (!Day.HasValue && Month.HasValue && Year.HasValue)
                {
                    yield return new ValidationResult($"{CurrentPage.FriendlyFieldName} must include a day", new[] { nameof(DateUtc) });
                }
                if (Day.HasValue && !Month.HasValue && Year.HasValue)
                {
                    yield return new ValidationResult($"{CurrentPage.FriendlyFieldName} must include a month", new[] { nameof(DateUtc) });
                }
                if (Day.HasValue && Month.HasValue && !Year.HasValue)
                {
                    yield return new ValidationResult($"{CurrentPage.FriendlyFieldName} must include a year", new[] { nameof(DateUtc) });
                }
                if (!Day.HasValue && !Month.HasValue && Year.HasValue)
                {
                    yield return new ValidationResult($"{CurrentPage.FriendlyFieldName} must include a day and month", new[] { nameof(DateUtc) });
                }
                if (!Day.HasValue && Month.HasValue && !Year.HasValue)
                {
                    yield return new ValidationResult($"{CurrentPage.FriendlyFieldName} must include a day and year", new[] { nameof(DateUtc) });
                }
                if (Day.HasValue && !Month.HasValue && !Year.HasValue)
                {
                    yield return new ValidationResult($"{CurrentPage.FriendlyFieldName} must include a month and year", new[] { nameof(DateUtc) });
                }
                if (!DateTime.TryParseExact(dateToValidate, "d/M/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out userInputDate)  && Day.HasValue && Month.HasValue && Year.HasValue )
                {
                    yield return new ValidationResult($"{CurrentPage.FriendlyFieldName} must be a real date", new[] { nameof(DateUtc) });
                }
            }
        }
        public override string GetData()
        {
            return DateUtc;
        }
    }
}

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
            if (Mandatory == true)
            {

                if (Day.HasValue && Month.HasValue && Year.HasValue)
                {
                     DateUtc = $"{Day}/{Month}/{Year}";
                }
                var clock = DateTime.UtcNow;
                if (string.IsNullOrEmpty(DateUtc))
                {
                    yield return new ValidationResult("Please enter in a date", new[] { nameof(DateUtc) });
                    
                }
                DateTime startDate;
                string dateToValidate = $"{Day}/{Month}/{Year}";
                if (!DateTime.TryParseExact(dateToValidate, "d/M/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out startDate)  && !string.IsNullOrEmpty(DateUtc))
                {
                    yield return new ValidationResult("Date is not valid.", new[] { nameof(DateUtc) });
                }

                if (DateTime.TryParseExact(dateToValidate, "d/M/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out startDate) && clock > startDate && !string.IsNullOrEmpty(DateUtc))
                {
                    yield return new ValidationResult("Date cannot be in the past.", new[] { nameof(DateUtc) });
                }
            }
        }
        public override string GetData()
        {
            return DateUtc;
        }
    }
}

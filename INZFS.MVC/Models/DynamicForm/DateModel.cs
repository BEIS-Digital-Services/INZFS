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



        

public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Mandatory == true)
            {

                if (Day.HasValue && Month.HasValue && Year.HasValue)
                {
                     DateUtc = $"{Day}/{Month}/{ Year}";
                }
                var clock = DateTime.UtcNow;
                if (string.IsNullOrEmpty(DateUtc))
                {
                    yield return new ValidationResult("Enter Valid Date", new[] { nameof(DateUtc) });
                }
                DateTime startDate;
                if (!DateTime.TryParseExact($"{Day}-{Month}-{Year}", "dd-MM-yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out startDate))
                {
                    yield return new ValidationResult("Date is not valid.", new[] { nameof(DateUtc) });
                }

                if (clock > startDate)
                {
                    yield return new ValidationResult("Date cannot be in the past.", new[] { nameof(DateUtc) });
                }
            }
        }

        

    }
}

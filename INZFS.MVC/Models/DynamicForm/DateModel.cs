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
        public string StartDateUtc { get; set; }
        public int? Day { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Mandatory == true)
            {
                var clock = DateTime.UtcNow;
                if (string.IsNullOrEmpty(StartDateUtc))
                {
                    yield return new ValidationResult("Enter estimated start date.", new[] { nameof(StartDateUtc) });
                }
                DateTime startDate;
                if (!DateTime.TryParseExact($"{Day}-{Month}-{Year}", "dd-MM-yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out startDate))
                {
                    yield return new ValidationResult("The start date is not valid.", new[] { nameof(StartDateUtc) });
                }

                if (clock > startDate)
                {
                    yield return new ValidationResult("The start date cannot be in the past.", new[] { nameof(StartDateUtc) });
                }
            }
        }

    }
}

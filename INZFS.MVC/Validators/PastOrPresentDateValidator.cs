using INZFS.MVC.Models.Application;
using INZFS.MVC.Models.DynamicForm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace INZFS.MVC.Validators
{
    public class PastOrPresentDateValidator : ICustomValidator
    {
        public IEnumerable<ValidationResult> Validate(BaseModel model, Page currentPage)
        {
            var dataInput = model.GetData();
            if (!string.IsNullOrEmpty(dataInput))
            {
                DateTime userDateInput;
                DateTime currentDate = DateTime.UtcNow.Date;
                if (DateTime.TryParseExact(dataInput, "d/M/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out userDateInput))
                {
                    if (userDateInput > currentDate)
                    {
                        yield return new ValidationResult($"{currentPage.FriendlyFieldName} must be in the past", new[] { "DateUtc" });
                    }
                }
            }
        }
    }
}

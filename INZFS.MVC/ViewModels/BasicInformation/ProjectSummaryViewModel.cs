using INZFS.MVC.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace INZFS.MVC.ViewModels
{
    public class ProjectSummaryViewModel : ProjectSummaryPart//, IValidatableObject
    {
        public string StartDateUtc { get; set; }

        [BindNever]
        public ProjectSummaryPart ProjectSummaryPart { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var localizer = validationContext.GetService<IStringLocalizer<ProjectSummaryViewModel>>();
            var clock = validationContext.GetService<OrchardCore.Modules.IClock>();
            if(string.IsNullOrEmpty(StartDateUtc))
            {
                yield return new ValidationResult(localizer["Enter estimated start date."], new[] { nameof(StartDateUtc) });
            }
            DateTime startDate;
            if (!DateTime.TryParseExact($"{Day}-{Month}-{Year}", "dd-MM-yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out startDate))
            {
                yield return new ValidationResult(localizer["The start date is not valid."], new[] { nameof(StartDateUtc) });
            }

            if (clock.UtcNow > startDate)
            {
                yield return new ValidationResult(localizer["The start date cannot be in the past."], new[] { nameof(StartDateUtc) });
            }

        }
    }
}

using INZFS.MVC.Models.Application;
using INZFS.MVC.Models.DynamicForm;
using INZFS.MVC.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Security.Claims;

namespace INZFS.MVC.Validators
{
    public class ProposalSummaryStartDateValidator : ICustomValidator
    {
        private readonly IContentRepository _contentRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDefinition _applicationDefinition;

        public ProposalSummaryStartDateValidator(IContentRepository contentRepository,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDefinition applicationDefinition)
        {
            _contentRepository = contentRepository;
            _httpContextAccessor = httpContextAccessor;
            _applicationDefinition = applicationDefinition;
        }
        public IEnumerable<ValidationResult> Validate(BaseModel model, Page currentPage)
        {
            var dataInput = model.GetData();
            bool isValid = true;

            if (!string.IsNullOrEmpty(dataInput))
            {
                DateTime startDate;
                DateTime endDate;
                if (DateTime.TryParseExact(dataInput, "d/M/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out startDate))
                {
                    if (startDate > new DateTime(2023, 3, 31))
                    {
                        isValid = false;
                        yield return new ValidationResult($"{currentPage.FriendlyFieldName} must be the same as or before 31st March 2023", new[] { "DateUtc" });
                    }
                    if (startDate < new DateTime(2022, 1, 31))
                    {
                        isValid = false;
                        yield return new ValidationResult($"{currentPage.FriendlyFieldName} must be after the 31st January 2022", new[] { "DateUtc" });
                    }

                    if (currentPage.FieldValidationDependsOn?.Count() > 0 && isValid)
                    {
                        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier); ;
                        var content = _contentRepository.GetApplicationContent(userId).Result;

                        // Get dependendant field data
                        var field = content.Fields.FirstOrDefault(f => f.Name.ToLower().Equals(currentPage.FieldValidationDependsOn[0].Trim().ToLower()));
                        if (!string.IsNullOrEmpty(field?.Data))
                        {
                            if (DateTime.TryParseExact(field.Data, "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
                            {
                                if (endDate < startDate)
                                {
                                    yield return new ValidationResult($"{currentPage.FriendlyFieldName} must be before project end date", new[] { "DateUtc" });
                                }
                            }
                         
                        }

                    }
                }
            }
        }
    }
}

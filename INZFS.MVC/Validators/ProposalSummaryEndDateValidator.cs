using INZFS.MVC.Models.DynamicForm;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Validators
{
    public class ProposalSummaryEndDateValidator : ICustomValidator
    {
        private readonly IContentRepository _contentRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDefinition _applicationDefinition;
        
        public ProposalSummaryEndDateValidator(IContentRepository contentRepository,
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
                DateTime endDate;
                if (DateTime.TryParseExact(dataInput, "d/M/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out endDate))
                {
                    if (endDate > new DateTime(2025, 3, 31))
                    {
                        isValid = false;
                        yield return new ValidationResult($"{currentPage.FriendlyFieldName} must be the same as or before 31st March 2025", new[] { "DateUtc" });
                    }
                    if (endDate < new DateTime(2022, 3, 31))
                    {
                        isValid = false;
                        yield return new ValidationResult($"{currentPage.FriendlyFieldName} must be after the 31st March 2022", new[] { "DateUtc" });
                    }

                    if(currentPage.FieldValidationDependsOn?.Count() > 0 && isValid)
                    {
                        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier); ;
                        var content = _contentRepository.GetApplicationContent(userId).Result;

                        // Get dependendant field data
                        var field = content.Fields.FirstOrDefault(f => f.Name.ToLower().Equals(currentPage.FieldValidationDependsOn[0].Trim().ToLower()));
                        if(!string.IsNullOrEmpty(field?.Data))
                        {
                            var startDate = DateTime.Parse(field.Data);
                            if(endDate < startDate)
                            {
                                yield return new ValidationResult($"{currentPage.FriendlyFieldName} must be after project start date", new[] { "DateUtc" });
                            }
                        }

                    }
                }
            }
        }
    }
}

using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models.DynamicForm
{
    public class StaticPageModel : BaseModel
    {
        protected override IEnumerable<ValidationResult> ExtendedValidation(ValidationContext validationContext)
        {
            if (Mandatory == true)
            {
                if (string.IsNullOrEmpty(DataInput))
                {
                    yield return new ValidationResult(ErrorMessage, new[] { nameof(DataInput) });
                }
            }
        }
    }

}

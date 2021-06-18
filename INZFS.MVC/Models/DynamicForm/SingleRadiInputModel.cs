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
    public class SingleRadiInputModel : BaseModel, IValidatableObject
    {
        public string DataInput { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Mandatory == true)
            {

                if (string.IsNullOrEmpty(DataInput))
                {
                    yield return new ValidationResult("Make a selection", new[] { nameof(DataInput) });
                }
            }
        }

    }
}

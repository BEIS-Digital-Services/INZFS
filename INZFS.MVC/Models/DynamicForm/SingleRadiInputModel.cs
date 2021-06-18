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
    public class SingleRadiInputModel : BaseModel
    {
        public int? DataInput { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Mandatory == true)
            {
                if (!DataInput.HasValue)
                {
                    yield return new ValidationResult("Enter Data", new[] { nameof(DataInput) });
                }
            }
        }

    }
}

using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models.DynamicForm
{
    public class FileUploadModel : BaseModel
    {
        public UploadedFile UploadedFile { get; set; }
        protected override IEnumerable<ValidationResult> ExtendedValidation(ValidationContext validationContext)
        {
            if (Mandatory == true && MarkAsComplete)
            {
                if (string.IsNullOrEmpty(DataInput))
                {
                    // yield return new ValidationResult(ErrorMessage, new[] { nameof(DataInput) });
                }
            }

            if (Mandatory == false && !FieldStatus.HasValue)
            {
                yield return new ValidationResult($"Please select Mark as complete or Not applicable option", new[] { nameof(DataInput) });
            }
        }
    }
}

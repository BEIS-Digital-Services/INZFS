using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace INZFS.MVC.Models.DynamicForm
{
    public class FileUploadModel : BaseModel
    {
        protected override IEnumerable<ValidationResult> ExtendedValidation(ValidationContext validationContext)
        {
            return Enumerable.Empty<ValidationResult>();
        }

        public override string GetData()
        {
            return UploadedFile?.Name;
        }
    }
}

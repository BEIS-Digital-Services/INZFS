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

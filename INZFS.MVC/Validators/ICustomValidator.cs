using INZFS.MVC.Models.Application;
using INZFS.MVC.Models.DynamicForm;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace INZFS.MVC.Validators
{
    public interface ICustomValidator
    {
        public IEnumerable<ValidationResult> Validate(BaseModel model, Page currentPage);
    }
}

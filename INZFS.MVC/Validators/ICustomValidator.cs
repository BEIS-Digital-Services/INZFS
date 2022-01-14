using INZFS.MVC.Models.DynamicForm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Validators
{
    public interface ICustomValidator
    {
        public IEnumerable<ValidationResult> Validate(BaseModel model, Page currentPage);
    }
}

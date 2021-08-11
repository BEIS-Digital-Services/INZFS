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

    public class MultiSelectInputModel : BaseModel, IValidatableObject
    {
        

        protected override IEnumerable<ValidationResult> ExtendedValidation(ValidationContext validationContext)
        {
            if (Mandatory == true )
            {
                if (UserOptionsSelected == null )
                {
                    yield return new ValidationResult(ErrorMessage, new[] { nameof(DataInput) });
                }
            }
        }


        public override List<String> GetSelectedByUser()
        {
            return UserOptionsSelected;
        }

    }
}

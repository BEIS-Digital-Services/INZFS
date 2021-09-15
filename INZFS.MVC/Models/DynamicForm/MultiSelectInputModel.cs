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

        public List<string> UserInput { get; set; }

        protected override IEnumerable<ValidationResult> ExtendedValidation(ValidationContext validationContext)
        {
            if (Mandatory == true  && MarkAsComplete)
            {
                if (UserInput == null )
                {
                    yield return new ValidationResult($"Select {CurrentPage.FriendlyFieldName.ToLower()} before marking as complete", new[] { nameof(DataInput) });
                }
            }
        }


        public override string GetData()
        {
            string UsersChoices = string.Join(",", UserInput);
            return UsersChoices;
        }
    }
}

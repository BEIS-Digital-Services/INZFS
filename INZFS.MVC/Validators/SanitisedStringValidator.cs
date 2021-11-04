using INZFS.MVC.Models.DynamicForm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Validators
{
    public class SanitisedStringValidator : ICustomValidator
    {
        public IEnumerable<ValidationResult> Validate(BaseModel model, Page currentPage)
        {
            var dataInput = model.GetData();
            if (!string.IsNullOrEmpty(dataInput))
            {
                string[] disallowedCharacters = { "<", ">" };

                foreach (var character in disallowedCharacters)
                {
                    if(dataInput.Contains(character)) {
                        yield return new ValidationResult($"{currentPage.FriendlyFieldName} must not contain character {character}", new[] { nameof(dataInput) });
                    }
                }
            }
        }
    }
}

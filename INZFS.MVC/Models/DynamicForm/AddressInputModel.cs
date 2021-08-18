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
    public class AddressInputModel : BaseModel, IValidatableObject
    {
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string PostCode { get; set; }

        public List<string> addresslist = new List<string>();


        protected override IEnumerable<ValidationResult> ExtendedValidation(ValidationContext validationContext)
        {
            

            if (Mandatory == true)
            {
                if ((AddressLine1 == null && AddressLine2 == null))
                {
                    yield return new ValidationResult($"Please add your Address to the Address field", new[] { nameof(DataInput) });
                }
                if (City == null)
                {
                    yield return new ValidationResult($"Please add the current city or town your business is based in into the Town or City field", new[] { nameof(DataInput) });
                }
                if (PostCode == null)
                {
                    yield return new ValidationResult($"Please add your postcode into the Postcode field", new[] { nameof(DataInput) });
                }
                if (PostCode.Length > 10)
                {
                    yield return new ValidationResult($"You have entered a postcode which is {PostCode.Length} characters in length. Please enter a postcode which is below 10 characters in length", new[] { nameof(DataInput) });

                }
                else
                {
                    addresslist.Add(AddressLine1);
                    addresslist.Add(AddressLine2);
                    addresslist.Add(City);
                    addresslist.Add(County);
                    addresslist.Add(PostCode);
                }
               
            }
        }
        public override string GetData()
        {
            string Address = string.Join(",", addresslist);
            return Address;
        }
    }
}

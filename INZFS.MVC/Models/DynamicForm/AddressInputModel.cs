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
                addresslist.Add(AddressLine1);
                addresslist.Add(AddressLine2);
                addresslist.Add(City);
                addresslist.Add(County);
                addresslist.Add(PostCode);

                if (addresslist == null)
                {
                    yield return new ValidationResult(ErrorMessage, new[] { nameof(DataInput) });
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

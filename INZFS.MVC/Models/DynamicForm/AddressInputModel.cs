using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            

            if (Mandatory == true && MarkAsComplete)
            {
                if ((AddressLine1 == null))
                {
                    yield return new ValidationResult($"Please add your address to the address field", new[] { nameof(AddressLine1) });
                }
                if (City == null)
                {
                    yield return new ValidationResult($"Please add the current city or town your business is based in into the town or city field", new[] { nameof(City) });
                }
                if (PostCode == null)
                {
                    yield return new ValidationResult($"Please add your postcode into the Postcode field", new[] { nameof(PostCode) });
                }
                if (PostCode != null && PostCode.Trim().Length > 8)
                {
                    yield return new ValidationResult($"You have entered a postcode which is {PostCode.Length} characters in length. Please enter a postcode which is below 8 characters in length", new[] { nameof(PostCode) });
                }
                else if (PostCode != null && !Regex.Match(PostCode.Trim(), @"[A-Za-z]{1,2}[0-9Rr][0-9A-Za-z]? [0-9][ABD-HJLNP-UW-Zabd-hjlnp-uw-z]{2}", RegexOptions.IgnoreCase).Success)
                {
                    yield return new ValidationResult($"Enter your postcode in the requested format, for instance: AA1 1AA", new[] { nameof(PostCode) });
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
            return string.Join(",", addresslist);
        }
    }
}

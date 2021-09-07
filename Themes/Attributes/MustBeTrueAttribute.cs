using System.ComponentModel.DataAnnotations;

namespace INZFS.Theme.Attributes
{
    public class MustBeTrueAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value is true;
        }
    }
}
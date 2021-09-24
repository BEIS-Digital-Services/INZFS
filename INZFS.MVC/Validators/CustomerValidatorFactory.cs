using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Validators
{
    public interface ICustomerValidatorFactory
    {
        //ICustomValidator Get(Type type);
        ICustomValidator Get(string type);
    }

    public class CustomerValidatorFactory : ICustomerValidatorFactory
    {
        private readonly IEnumerable<ICustomValidator> _customValidators;

        public CustomerValidatorFactory(IEnumerable<ICustomValidator> customValidators)
        {
            _customValidators = customValidators;
        }

        public ICustomValidator Get(string type)
        {
            var customValidator = _customValidators.SingleOrDefault(x => x.GetType().Name.ToLower() == type.ToLower());
            if (customValidator == null)
                throw new ArgumentOutOfRangeException(type,
                    $"Custom validator cannot be found for type: {type}");
            return customValidator;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.Theme.ViewModels
{
    public class RegistrationOrganisationViewModel
    {
        [RegularExpression(@"^.{0,160}$", ErrorMessage = "The organisation's name must be 160 characters or fewer")]
        public string OrganisationName { get; set; }
    }
}

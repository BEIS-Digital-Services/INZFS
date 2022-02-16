using System.ComponentModel.DataAnnotations;

namespace INZFS.Theme.ViewModels
{
    public class RegistrationOrganisationViewModel
    {
        [RegularExpression(@"^.{0,160}$", ErrorMessage = "The organisation's name must be 160 characters or fewer")]
        public string OrganisationName { get; set; }
    }
}

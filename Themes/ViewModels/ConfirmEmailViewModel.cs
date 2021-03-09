using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users;

namespace INZFS.Theme.ViewModels
{
    public class ConfirmEmailViewModel : ShapeViewModel
    {
        public ConfirmEmailViewModel()
        {
            Metadata.Type = "TemplateUserConfirmEmail";
        }

        public IUser User { get; set; }
        public string ConfirmEmailUrl { get; set; }
    }
}

using INZFS.Templates.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace INZFS.Templates.ViewModels
{
    public class EditGovTextFieldViewModel
    {
        public string Text { get; set; }
        public GovTextField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
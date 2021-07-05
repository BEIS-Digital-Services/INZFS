using INZFS.Templates.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace INZFS.Templates.ViewModels
{
    public class DisplayGovTextFieldViewModel
    {
        public string Text => Field.Text;
        public GovTextField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}

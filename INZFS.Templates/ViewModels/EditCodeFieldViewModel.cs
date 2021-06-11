using INZFS.Templates.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace INZFS.Templates.ViewModels
{
    public class EditCodeFieldViewModel
    {
        public CodeField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }

        public string Value { get; set; }
    }
}
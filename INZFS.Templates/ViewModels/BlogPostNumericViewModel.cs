using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace INZFS.Templates.ViewModels
{
    public class DisplayNumericFieldViewModel
    {
        public decimal? Value => Field.Value;
        public NumericField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
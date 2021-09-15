using Newtonsoft.Json;

namespace INZFS.Templates.Settings
{
    public class GovTextFieldPredefinedListEditorSettings
    {
        public ListValueOption[] Options { get; set; }
        public EditorOption Editor { get; set; }
        public string DefaultValue { get; set; }
    }

    public enum EditorOption
    {
        Radio,
        Dropdown
    }

    public class ListValueOption
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
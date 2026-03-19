namespace AdminPanel.ViewModels.Common
{
    public class FilterOption
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;

        public static FilterOption From(int id, string name)
            => new() { Value = id.ToString(), Label = name };

        public static FilterOption From(string value, string label)
            => new() { Value = value, Label = label };
    }

}

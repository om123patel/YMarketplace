namespace AdminPanel.ViewModels.Common
{
    public class DeleteModalModel
    {
        public string Title { get; set; } = "Delete item?";
        public string? Description { get; set; }
        public string? ButtonLabel { get; set; } = "Delete";
    }
}

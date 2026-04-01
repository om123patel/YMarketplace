namespace AdminPanel.ViewModels.Common
{
    public class ModalViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ActionUrl { get; set; }
        public string SubmitText { get; set; } = "Submit";

        // Raw HTML OR Partial View Name
        public string? BodyHtml { get; set; }

        // Optional: for advanced use
        public object? Data { get; set; }
    }
}

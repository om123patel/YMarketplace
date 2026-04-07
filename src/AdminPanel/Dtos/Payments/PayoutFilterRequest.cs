namespace AdminPanel.Dtos.Payments
{
    public class PayoutFilterRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Status { get; set; }
        public string? Search { get; set; }
    }
}
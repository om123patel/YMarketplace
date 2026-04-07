namespace AdminPanel.Dtos.Payments
{
    public class TransactionFilterRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Status { get; set; }
        public string? Method { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createdAt";
        public string SortDirection { get; set; } = "desc";
    }
}
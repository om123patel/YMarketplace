namespace AdminPanel.ViewModels.Payments
{
    public class CommissionRuleRowViewModel
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal RatePercent { get; set; }
        public bool IsActive { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Edit form fields (inline)
        public string? EditName { get; set; }
        public int? EditCategoryId { get; set; }
        public decimal EditRatePercent { get; set; }
    }

}

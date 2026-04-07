namespace AdminPanel.Dtos.Payments
{
    public class UpdateCommissionRuleRequest
    {
        public string Name { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public decimal RatePercent { get; set; }
    }
}
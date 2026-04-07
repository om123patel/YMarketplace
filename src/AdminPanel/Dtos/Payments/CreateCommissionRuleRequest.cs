namespace AdminPanel.Dtos.Payments
{
    public class CreateCommissionRuleRequest
    {
        public string Name { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public decimal RatePercent { get; set; }
    }
}
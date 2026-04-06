namespace Payments.Application.DTOs.CommissionRules
{
    public class CreateCommissionRuleDto
    {
        public int? CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal RatePercent { get; set; }
    }
}
namespace Payments.Application.DTOs.Payouts
{
    public class ProcessPayoutDto
    {
        public string GatewayReference { get; set; } = string.Empty;
        public string? AdminNote { get; set; }
    }
}
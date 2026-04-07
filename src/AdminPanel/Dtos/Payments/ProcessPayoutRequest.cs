namespace AdminPanel.Dtos.Payments
{
    public class ProcessPayoutRequest
    {
        public string GatewayReference { get; set; } = string.Empty;
        public string? AdminNote { get; set; }
    }
}
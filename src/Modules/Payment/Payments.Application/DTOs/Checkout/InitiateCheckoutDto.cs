namespace Payments.Application.DTOs.Checkout
{
    public class InitiateCheckoutDto
    {
        public Guid OrderId { get; set; }
        public string Gateway { get; set; } = "Razorpay";  // Razorpay | Cashfree | PayU | PhonePe | Paytm
        public string? PreferredMethod { get; set; }       // UPI | Card | NetBanking | Wallet
        public string SuccessUrl { get; set; } = string.Empty;
        public string FailureUrl { get; set; } = string.Empty;
    }
}
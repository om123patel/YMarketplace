namespace Payments.Application.DTOs.Transactions
{
    public class SellerEarningsSummary
    {
        public Guid SellerId { get; set; }
        public decimal TotalEarned { get; set; }  // all completed transactions
        public decimal TotalCommission { get; set; }  // total deducted as commission
        public decimal TotalPaidOut { get; set; }  // sum of completed payouts
        public decimal AvailableBalance { get; set; }  // TotalEarned - TotalPaidOut
        public string CurrencyCode { get; set; } = "INR";
    }
}
namespace Payments.Application.DTOs.Transactions
{
    public class CreateTransactionDto
    {
        public Guid OrderId { get; set; }
        public Guid BuyerId { get; set; }
        public Guid SellerId { get; set; }
        public Guid StoreId { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = "INR";
        public string Method { get; set; } = "Card";
        public int? CategoryId { get; set; }   // used to look up commission rate
        public string? GatewayProvider { get; set; }
    }
}
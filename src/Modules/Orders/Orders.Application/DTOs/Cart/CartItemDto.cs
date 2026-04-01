namespace Orders.Application.DTOs.Cart
{
    public class CartItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? VariantName { get; set; }
        public string? ProductImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }

}

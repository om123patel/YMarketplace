namespace Orders.Application.DTOs.Cart
{
    public class AddToCartDto
    {
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? VariantName { get; set; }
        public string? ProductImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public string CurrencyCode { get; set; } = "INR";
        public int Quantity { get; set; } = 1;
    }

}

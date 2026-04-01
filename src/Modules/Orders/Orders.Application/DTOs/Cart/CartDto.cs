namespace Orders.Application.DTOs.Cart
{
    public class CartDto
    {
        public Guid Id { get; set; }
        public Guid BuyerId { get; set; }
        public List<CartItemDto> Items { get; set; } = [];
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
    }
}

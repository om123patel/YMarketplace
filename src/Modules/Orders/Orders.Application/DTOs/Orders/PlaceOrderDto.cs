namespace Orders.Application.DTOs.Orders
{
    public class PlaceOrderDto
    {
        public Guid StoreId { get; set; }
        public Guid SellerId { get; set; }
        public string CurrencyCode { get; set; } = "INR";
        public decimal ShippingAmount { get; set; }

        // Shipping address
        public string? ShippingName { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? Phone { get; set; }

        public List<PlaceOrderItemDto> Items { get; set; } = [];
    }
}

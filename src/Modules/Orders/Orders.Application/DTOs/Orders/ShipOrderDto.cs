namespace Orders.Application.DTOs.Orders
{
    public class ShipOrderDto
    {
        public string TrackingNumber { get; set; } = string.Empty;
        public string Carrier { get; set; } = string.Empty;
        public string? TrackingUrl { get; set; }
    }
}

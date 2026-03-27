namespace Identity.Application.DTOs.Seller
{
    public class UpdateSellerDto
    {
        public Guid Id { get; set; }
        public string BusinessName { get; set; }
        public string? BusinessEmail { get; set; }
        public string? BusinessPhone { get; set; }
        public string? Description { get; set; }
        public string? WebsiteUrl { get; set; }
    }
}

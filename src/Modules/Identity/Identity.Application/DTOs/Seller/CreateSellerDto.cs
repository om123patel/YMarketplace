namespace Identity.Application.DTOs.Seller
{
    public class CreateSellerDto
    {
        public Guid UserId { get; set; }
        public string BusinessName { get; set; }
        public string? BusinessEmail { get; set; }
        public string? BusinessPhone { get; set; }
        public string? Description { get; set; }
        public string? WebsiteUrl { get; set; }

        public AddressDto Address { get; set; }
    }
    public class AddressDto
    {
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
    }
}

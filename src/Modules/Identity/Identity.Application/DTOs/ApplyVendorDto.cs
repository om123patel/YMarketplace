namespace Identity.Application.DTOs
{
    public class ApplyVendorDto
    {
        public string StoreName { get; set; } = string.Empty;
        public string BusinessType { get; set; } = string.Empty;
        public string? TaxId { get; set; }
        public string? ContactPhone { get; set; }
        public string? Description { get; set; }
    }

}

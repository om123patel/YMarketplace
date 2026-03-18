namespace Identity.Application.DTOs
{
    public class VendorApplicationDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ApplicantName { get; set; } = string.Empty;
        public string ApplicantEmail { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string BusinessType { get; set; } = string.Empty;
        public string? TaxId { get; set; }
        public string? ContactPhone { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

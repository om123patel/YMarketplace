namespace Catalog.Application.DTOs
{
    public class ProductStatusHistoryDto
    {
        public int Id { get; set; }
        public Guid ProductId { get; set; }
        public string? FromStatus { get; set; }
        public string ToStatus { get; set; } = string.Empty;
        public string? Note { get; set; }
        public Guid ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
    }

}

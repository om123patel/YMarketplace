namespace Catalog.Application.DTOs.Wishlist
{
    public class WishlistItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        // Product snapshot (populated by service join)
        public string? ProductName { get; set; }
        public string? ProductSlug { get; set; }
        public decimal? BasePrice { get; set; }
        public string? CurrencyCode { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime AddedAt { get; set; }
    }
}

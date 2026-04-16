namespace Catalog.Application.DTOs.Wishlist
{
    public class WishlistDto
    {
        public Guid Id { get; set; }
        public Guid BuyerId { get; set; }
        public List<WishlistItemDto> Items { get; set; } = [];
        public int TotalItems => Items.Count;
    }
}

namespace Catalog.Domain.Exceptions
{
    public class ProductNotFoundException : CatalogException
    {
        public ProductNotFoundException(Guid productId)
            : base("PRODUCT_NOT_FOUND", $"Product with id '{productId}' was not found.") { }
    }
}

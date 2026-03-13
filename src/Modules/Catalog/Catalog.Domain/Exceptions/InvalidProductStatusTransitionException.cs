using Catalog.Domain.Enums;

namespace Catalog.Domain.Exceptions
{
    public class InvalidProductStatusTransitionException : CatalogException
    {
        public InvalidProductStatusTransitionException(
            ProductStatus from, ProductStatus to)
            : base(
                "INVALID_STATUS_TRANSITION",
                $"Cannot transition product status from '{from}' to '{to}'.")
        { }
    }
}

namespace Catalog.Domain.Exceptions
{
    public class CategoryNotFoundException : CatalogException
    {
        public CategoryNotFoundException(int categoryId)
            : base("CATEGORY_NOT_FOUND", $"Category with id '{categoryId}' was not found.") { }
    }
}

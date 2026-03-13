using Shared.Domain.Exceptions;

namespace Catalog.Domain.Exceptions
{
    public class CatalogException : DomainException
    {
        public CatalogException(string code, string message)
            : base(code, message) { }
    }
}

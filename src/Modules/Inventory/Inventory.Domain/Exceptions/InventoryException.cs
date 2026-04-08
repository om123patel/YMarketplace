using Shared.Domain.Exceptions;

namespace Inventory.Domain.Exceptions
{
    public class InventoryException : DomainException
    {
        public InventoryException(string code, string message)
            : base(code, message) { }
    }
}
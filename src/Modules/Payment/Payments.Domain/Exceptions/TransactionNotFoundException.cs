namespace Payments.Domain.Exceptions
{
    public class TransactionNotFoundException : PaymentsException
    {
        public TransactionNotFoundException(Guid id)
            : base("TRANSACTION_NOT_FOUND",
                   $"Transaction '{id}' was not found.")
        { }
    }
}
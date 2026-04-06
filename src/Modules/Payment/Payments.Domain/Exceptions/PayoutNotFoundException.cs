namespace Payments.Domain.Exceptions
{
    public class PayoutNotFoundException : PaymentsException
    {
        public PayoutNotFoundException(Guid id)
            : base("PAYOUT_NOT_FOUND",
                   $"Payout '{id}' was not found.")
        { }
    }
}
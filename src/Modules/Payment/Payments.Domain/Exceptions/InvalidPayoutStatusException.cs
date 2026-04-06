using Payments.Domain.Enums;

namespace Payments.Domain.Exceptions
{
    public class InvalidPayoutStatusException : PaymentsException
    {
        public InvalidPayoutStatusException(PayoutStatus from, PayoutStatus to)
            : base("INVALID_PAYOUT_STATUS_TRANSITION",
                   $"Cannot transition payout from '{from}' to '{to}'.")
        { }
    }
}
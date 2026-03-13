using Shared.Domain.Abstractions;

namespace Shared.Domain.Primitives
{
    public sealed class Money : ValueObject
    {
        public decimal Amount { get; }
        public string CurrencyCode { get; }  // ISO 4217: USD, EUR, GBP

        private Money() { }  // EF Core

        public Money(decimal amount, string currencyCode)
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative.", nameof(amount));

            if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Length != 3)
                throw new ArgumentException("Currency code must be 3 characters.", nameof(currencyCode));

            Amount = amount;
            CurrencyCode = currencyCode.ToUpperInvariant();
        }

        public static Money Zero(string currencyCode) => new(0, currencyCode);

        public Money Add(Money other)
        {
            if (CurrencyCode != other.CurrencyCode)
                throw new InvalidOperationException("Cannot add money with different currencies.");
            return new Money(Amount + other.Amount, CurrencyCode);
        }

        public Money Subtract(Money other)
        {
            if (CurrencyCode != other.CurrencyCode)
                throw new InvalidOperationException("Cannot subtract money with different currencies.");
            return new Money(Amount - other.Amount, CurrencyCode);
        }

        public override string ToString() => $"{Amount:F2} {CurrencyCode}";

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return CurrencyCode;
        }
    }
}

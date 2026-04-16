// src/Modules/Identity/Identity.Domain/Entities/CustomerWallet.cs

using Identity.Domain.Enums;
using Shared.Domain.Abstractions;
using Shared.Domain.Exceptions;

namespace Identity.Domain.Entities
{
    public class CustomerWallet : AggregateRoot<Guid>
    {
        public Guid UserId { get; private set; }
        public decimal Balance { get; private set; }
        public string CurrencyCode { get; private set; } = "INR";
        public bool IsActive { get; private set; }

        public ICollection<WalletTransaction> Transactions { get; private set; } = [];

        private CustomerWallet() { } // EF Core

        public static CustomerWallet Create(Guid userId, Guid createdBy,
            string currencyCode = "INR")
        {
            return new CustomerWallet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Balance = 0,
                CurrencyCode = currencyCode,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }

        public WalletTransaction Credit(
            decimal amount, string description,
            string? referenceId, Guid createdBy)
        {
            if (amount <= 0)
                throw new DomainException(
                    "INVALID_AMOUNT", "Credit amount must be greater than zero.");

            Balance += amount;
            SetUpdatedBy(createdBy);

            var txn = WalletTransaction.Create(
                Id, amount, WalletTransactionType.Credit,
                description, referenceId, Balance, createdBy);

            Transactions.Add(txn);
            return txn;
        }

        public WalletTransaction Debit(
            decimal amount, string description,
            string? referenceId, Guid createdBy)
        {
            if (amount <= 0)
                throw new DomainException(
                    "INVALID_AMOUNT", "Debit amount must be greater than zero.");

            if (amount > Balance)
                throw new DomainException(
                    "INSUFFICIENT_BALANCE",
                    $"Insufficient wallet balance. Available: {Balance} {CurrencyCode}.");

            Balance -= amount;
            SetUpdatedBy(createdBy);

            var txn = WalletTransaction.Create(
                Id, amount, WalletTransactionType.Debit,
                description, referenceId, Balance, createdBy);

            Transactions.Add(txn);
            return txn;
        }
    }

   

    
}
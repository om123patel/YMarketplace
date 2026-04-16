// src/Modules/Identity/Identity.Domain/Entities/CustomerAddress.cs

using Shared.Domain.Abstractions;

namespace Identity.Domain.Entities
{
    public class CustomerAddress : Entity<Guid>
    {
        public Guid UserId { get; private set; }
        public string FullName { get; private set; } = string.Empty;
        public string Phone { get; private set; } = string.Empty;
        public string AddressLine1 { get; private set; } = string.Empty;
        public string? AddressLine2 { get; private set; }
        public string City { get; private set; } = string.Empty;
        public string State { get; private set; } = string.Empty;
        public string PostalCode { get; private set; } = string.Empty;
        public string Country { get; private set; } = string.Empty;
        public bool IsDefault { get; private set; }
        public string? Label { get; private set; } // Home | Work | Other

        private CustomerAddress() { } // EF Core

        public static CustomerAddress Create(
            Guid userId,
            string fullName,
            string phone,
            string addressLine1,
            string? addressLine2,
            string city,
            string state,
            string postalCode,
            string country,
            bool isDefault,
            string? label,
            Guid createdBy)
        {
            return new CustomerAddress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FullName = fullName,
                Phone = phone,
                AddressLine1 = addressLine1,
                AddressLine2 = addressLine2,
                City = city,
                State = state,
                PostalCode = postalCode,
                Country = country,
                IsDefault = isDefault,
                Label = label,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }

        public void Update(
            string fullName, string phone,
            string addressLine1, string? addressLine2,
            string city, string state, string postalCode,
            string country, string? label, Guid updatedBy)
        {
            FullName = fullName;
            Phone = phone;
            AddressLine1 = addressLine1;
            AddressLine2 = addressLine2;
            City = city;
            State = state;
            PostalCode = postalCode;
            Country = country;
            Label = label;
            SetUpdatedBy(updatedBy);
        }

        public void SetAsDefault() => IsDefault = true;
        public void UnsetAsDefault() => IsDefault = false;
    }
}
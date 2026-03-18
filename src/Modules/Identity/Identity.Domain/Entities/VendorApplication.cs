using Identity.Domain.Enums;
using Shared.Domain.Abstractions;
using Shared.Domain.Exceptions;

namespace Identity.Domain.Entities
{
    public class VendorApplication : Entity<Guid>
    {
        public Guid UserId { get; private set; }
        public string StoreName { get; private set; } = string.Empty;
        public string BusinessType { get; private set; } = string.Empty;
        public string? TaxId { get; private set; }
        public string? ContactPhone { get; private set; }
        public string? Description { get; private set; }

        public VendorApplicationStatus Status { get; private set; }

        public Guid? ReviewedByAdminId { get; private set; }
        public DateTime? ReviewedAt { get; private set; }
        public string? RejectionReason { get; private set; }

        private VendorApplication() { } // EF Core

        public static VendorApplication Create(
            Guid userId,
            string storeName,
            string businessType,
            Guid createdBy,
            string? taxId = null,
            string? contactPhone = null,
            string? description = null)
        {
            return new VendorApplication
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StoreName = storeName.Trim(),
                BusinessType = businessType.Trim(),
                TaxId = taxId,
                ContactPhone = contactPhone,
                Description = description,
                Status = VendorApplicationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }

        public void Approve(Guid adminId)
        {
            if (Status != VendorApplicationStatus.Pending)
                throw new DomainException(
                    "INVALID_STATUS_TRANSITION",
                    "Only pending applications can be approved.");

            Status = VendorApplicationStatus.Approved;
            ReviewedByAdminId = adminId;
            ReviewedAt = DateTime.UtcNow;
            SetUpdatedBy(adminId);
        }

        public void Reject(Guid adminId, string reason)
        {
            if (Status != VendorApplicationStatus.Pending)
                throw new DomainException(
                    "INVALID_STATUS_TRANSITION",
                    "Only pending applications can be rejected.");

            Status = VendorApplicationStatus.Rejected;
            ReviewedByAdminId = adminId;
            ReviewedAt = DateTime.UtcNow;
            RejectionReason = reason;
            SetUpdatedBy(adminId);
        }
    }

}

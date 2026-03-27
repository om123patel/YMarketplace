using Identity.Domain.Enums;
using Shared.Domain.Abstractions;
using Shared.Domain.Primitives;

namespace Identity.Domain.Entities
{
    public class Seller : AggregateRoot<Guid>, IConcurrencyToken
    {
        public Seller() { }
        public Guid UserId { get; private set; }

        public string BusinessName { get; private set; }
        public string? BusinessEmail { get; private set; }
        public string? BusinessPhone { get; private set; }
        public string? Description { get; private set; }

        public string? LogoUrl { get; private set; }
        public string? WebsiteUrl { get; private set; }

        public Address Address { get; private set; }

        public SellerStatus Status { get; private set; }

        public Guid? ApprovedByAdminId { get; private set; }
        public DateTime? ApprovedAt { get; private set; }

        public Guid? RejectedByAdminId { get; private set; }
        public DateTime? RejectedAt { get; private set; }
        public string? RejectionReason { get; private set; }

        public int TotalProducts { get; private set; }
        public int TotalOrders { get; private set; }
        public decimal TotalRevenue { get; private set; }

        public decimal? Rating { get; private set; }

        public byte[]? RowVersion { get; private set; }

        public User User { get; private set; }

        #region Factory

        public static Seller Create(
            Guid userId,
            string businessName,
            string? email,
            string? phone,
            Address address)
        {
            return new Seller
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BusinessName = businessName,
                BusinessEmail = email,
                BusinessPhone = phone,
                Address = address,
                Status = SellerStatus.PendingApproval,
                TotalProducts = 0,
                TotalOrders = 0,
                TotalRevenue = 0
            };
        }

        #endregion

        #region Behavior

        public void Approve(Guid adminId)
        {
            if (Status != SellerStatus.PendingApproval)
                throw new InvalidOperationException("Only pending sellers can be approved.");

            Status = SellerStatus.Active;
            ApprovedByAdminId = adminId;
            ApprovedAt = DateTime.UtcNow;
        }

        public void Reject(Guid adminId, string reason)
        {
            if (Status != SellerStatus.PendingApproval)
                throw new InvalidOperationException("Only pending sellers can be rejected.");

            Status = SellerStatus.Rejected;
            RejectedByAdminId = adminId;
            RejectedAt = DateTime.UtcNow;
            RejectionReason = reason;
        }

        public void Suspend()
        {
            if (Status != SellerStatus.Active)
                throw new InvalidOperationException("Only active sellers can be suspended.");

            Status = SellerStatus.Suspended;
        }

        public void Activate()
        {
            if (Status != SellerStatus.Suspended)
                throw new InvalidOperationException("Only suspended sellers can be activated.");

            Status = SellerStatus.Active;
        }

        public void UpdateProfile(
            string businessName,
            string? email,
            string? phone,
            string? description,
            string? websiteUrl)
        {
            BusinessName = businessName;
            BusinessEmail = email;
            BusinessPhone = phone;
            Description = description;
            WebsiteUrl = websiteUrl;
        }

        public void UpdateAddress(Address address)
        {
            Address = address;
        }

        public void AddProduct()
        {
            TotalProducts++;
        }

        public void AddOrder(decimal amount)
        {
            TotalOrders++;
            TotalRevenue += amount;
        }

        public void UpdateRating(decimal rating)
        {
            if (rating < 0 || rating > 5)
                throw new ArgumentOutOfRangeException(nameof(rating));

            Rating = rating;
        }

        #endregion
    }
}

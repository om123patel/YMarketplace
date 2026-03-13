namespace AdminPanel.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int ActiveProducts { get; set; }
        public int ActiveSellers { get; set; }
        public int PendingApprovals { get; set; }
        public List<RecentOrderItem> RecentOrders { get; set; } = [];
    }

    public class RecentOrderItem
    {
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

}

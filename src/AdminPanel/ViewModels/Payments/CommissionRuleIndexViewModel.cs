// src/AdminPanel/ViewModels/Payments/CommissionRuleIndexViewModel.cs
namespace AdminPanel.ViewModels.Payments
{
    public class CommissionRuleIndexViewModel
    {
        public List<CommissionRuleRowViewModel> Rules { get; set; } = [];

        // Create form fields
        public string? NewName { get; set; }
        public int? NewCategoryId { get; set; }
        public decimal NewRatePercent { get; set; }
    }
}
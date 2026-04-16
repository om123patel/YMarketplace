namespace Identity.Application.DTOs.Wallet
{
    public class WalletHistoryDto
    {
        public WalletDto Wallet { get; set; } = default!;
        public List<WalletTransactionDto> Transactions { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}

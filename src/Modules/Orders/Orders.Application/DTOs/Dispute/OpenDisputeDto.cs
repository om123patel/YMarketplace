namespace Orders.Application.DTOs.Dispute
{
    public class OpenDisputeDto
    {
        public string Reason { get; set; } = string.Empty;
        public string? Evidence { get; set; }
    }
}

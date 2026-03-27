namespace Web.API.Models
{
    public class RejectSellerRequest
    {
        public string Reason { get; set; }
    }

    public class SuspendSellerRequest
    {
        public string? Reason { get; set; }
    }
}

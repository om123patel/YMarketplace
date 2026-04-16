namespace Payments.Infrastructure.Gateways
{
    namespace Payments.Infrastructure.Gateways
    {

        public class PaytmOptions
        {
            public const string Section = "Gateways:Paytm";
            public string MerchantId { get; set; } = string.Empty;
            public string MerchantKey { get; set; } = string.Empty;
            public string WebhookKey { get; set; } = string.Empty;
            public bool IsProduction { get; set; } = false;
        }
    }
}

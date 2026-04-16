using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Infrastructure.Gateways.Models
{
    public class PayUOptions
    {
        public const string Section = "Gateways:PayU";
        public string MerchantKey { get; set; } = string.Empty;
        public string MerchantSalt { get; set; } = string.Empty;
        public string WebhookSalt { get; set; } = string.Empty;
        public bool IsProduction { get; set; } = false;
    }
}

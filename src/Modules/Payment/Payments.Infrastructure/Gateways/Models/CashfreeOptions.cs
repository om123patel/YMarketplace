using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Infrastructure.Gateways.Models
{
    public class CashfreeOptions
    {
        public const string Section = "Gateways:Cashfree";
        public string AppId { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public string Environment { get; set; } = "sandbox"; // sandbox | production
    }
}

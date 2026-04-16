using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Infrastructure.Gateways.Models
{
    public class RazorpayOptions
    {
        public const string Section = "Gateways:Razorpay";
        public string KeyId { get; set; } = string.Empty;
        public string KeySecret { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
    }
}

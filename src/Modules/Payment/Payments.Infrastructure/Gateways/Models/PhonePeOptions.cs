using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Infrastructure.Gateways.Models
{
    public class PhonePeOptions
    {
        public const string Section = "Gateways:PhonePe";
        public string MerchantId { get; set; } = string.Empty;
        public string SaltKey { get; set; } = string.Empty;
        public int SaltIndex { get; set; } = 1;
        public bool IsProduction { get; set; } = false;
    }
}

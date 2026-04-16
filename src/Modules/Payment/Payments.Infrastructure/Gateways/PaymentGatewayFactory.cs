using Payments.Application.Interfaces;

namespace Payments.Infrastructure.Gateways
{
    public class PaymentGatewayFactory : IPaymentGatewayFactory
    {
        private readonly IReadOnlyDictionary<string, IPaymentGateway> _gateways;

        public PaymentGatewayFactory(IEnumerable<IPaymentGateway> gateways)
        {
            _gateways = gateways.ToDictionary(g => g.GatewayName, StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyList<string> AvailableGateways => _gateways.Keys.ToList();

        public IPaymentGateway GetGateway(string gatewayName)
        {
            if (_gateways.TryGetValue(gatewayName, out var gateway))
                return gateway;

            throw new InvalidOperationException(
                $"Payment gateway '{gatewayName}' is not configured. Available: {string.Join(", ", _gateways.Keys)}");
        }
    }
}
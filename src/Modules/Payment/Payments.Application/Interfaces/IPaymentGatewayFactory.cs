namespace Payments.Application.Interfaces
{
    public interface IPaymentGatewayFactory
    {
        IPaymentGateway GetGateway(string gatewayName);
        IReadOnlyList<string> AvailableGateways { get; }
    }
}
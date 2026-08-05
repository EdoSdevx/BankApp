using TcmbSimulator.Contracts.Routing;

namespace TcmbSimulator.Services;

public interface IRecipientBankClient
{
    Task<RoutePaymentResponse> RouteAsync(
        RoutePaymentRequest request,
        string receiverApiBaseUrl,
        string sharedSecret,
        CancellationToken cancellationToken = default);
}

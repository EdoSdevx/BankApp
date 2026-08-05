using TcmbSimulator.Contracts.Routing;

namespace TcmbSimulator.Data;

public interface IRoutingOutboxDataAccess
{
    Task<List<PendingRoutingPayment>> GetPendingAsync(
        int batchSize,
        CancellationToken cancellationToken = default);

    Task MarkRoutingAsync(
        int routingOutboxMessageId,
        CancellationToken cancellationToken = default);

    Task MarkResultAsync(
        int routingOutboxMessageId,
        RoutePaymentResponse response,
        CancellationToken cancellationToken = default);

    Task MarkFailedAsync(
        int routingOutboxMessageId,
        string error,
        int maxAttempts,
        CancellationToken cancellationToken = default);
}

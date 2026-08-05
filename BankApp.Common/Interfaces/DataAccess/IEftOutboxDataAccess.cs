using BankApp.BankApp.Common.Dtos.Eft.Switch;

namespace BankApp.BankApp.Common.Interfaces.DataAccess;

public interface IEftOutboxDataAccess
{
    Task<List<PendingEftOutboxDto>> GetPendingAsync(
        int batchSize,
        CancellationToken cancellationToken = default);

    Task MarkSubmittedAsync(
        int outboxMessageId,
        string centralReference,
        CancellationToken cancellationToken = default);

    Task MarkFailedAsync(
        int outboxMessageId,
        string error,
        int maxAttempts,
        CancellationToken cancellationToken = default);
}

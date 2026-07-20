using BankApp.BankApp.Common.Dtos.Customer;

namespace BankApp.BankApp.Common.Interfaces.Services;

public interface IAdminApprovalService
{
    Task<Result<List<PendingTransferDto>>> GetPendingTransfersAsync(CancellationToken cancellationToken = default);
    Task<Result> ApproveTransferAsync(int pendingTransferId, int employeeId, CancellationToken cancellationToken = default);
    Task<Result> RejectTransferAsync(int pendingTransferId, int employeeId, string? reason, CancellationToken cancellationToken = default);
}

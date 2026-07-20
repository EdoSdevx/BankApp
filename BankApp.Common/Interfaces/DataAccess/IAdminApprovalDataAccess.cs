using BankApp.BankApp.Common.Dtos.Customer;

namespace BankApp.BankApp.Common.Interfaces.DataAccess;

public interface IAdminApprovalDataAccess
{
    Task<List<PendingTransferDto>> GetPendingTransfersAsync(CancellationToken cancellationToken = default);
    Task<ApprovalResultDto> ApproveTransferAsync(int pendingTransferId, int employeeId, CancellationToken cancellationToken = default);
    Task<ApprovalResultDto> RejectTransferAsync(int pendingTransferId, int employeeId, string? reason, CancellationToken cancellationToken = default);
}

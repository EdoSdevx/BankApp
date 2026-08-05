using BankApp.BankApp.Common.Dtos.Eft;

namespace BankApp.BankApp.Common.Interfaces.Services;

public interface IEftService
{
    Task<Result> CreateAsync(
        int customerId,
        CreateEftRequestDto dto,
        CancellationToken cancellationToken = default);

    Task<Result<List<EftDetailDto>>> GetByCustomerAsync(
        int customerId,
        CancellationToken cancellationToken = default);

    Task<Result<EftDetailDto>> GetByIdAsync(
        int customerId,
        int eftTransferId,
        CancellationToken cancellationToken = default);

    Task<Result<List<PendingEftDto>>> GetPendingAsync(
        CancellationToken cancellationToken = default);

    Task<Result<EftDetailDto>> ApproveAsync(
        int eftTransferId,
        int employeeId,
        CancellationToken cancellationToken = default);

    Task<Result<EftDetailDto>> RejectAsync(
        int eftTransferId,
        int employeeId,
        string? reason,
        CancellationToken cancellationToken = default);
}

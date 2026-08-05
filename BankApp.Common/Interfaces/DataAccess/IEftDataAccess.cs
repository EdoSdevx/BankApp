using BankApp.BankApp.Common.Dtos.Eft;

namespace BankApp.BankApp.Common.Interfaces.DataAccess;

public interface IEftDataAccess
{
    Task CreateAsync(
        int customerId,
        CreateEftRequestDto dto,
        string senderReference,
        CancellationToken cancellationToken = default);

    Task<List<EftDetailDto>> GetByCustomerAsync(
        int customerId,
        CancellationToken cancellationToken = default);

    Task<EftDetailDto?> GetByIdAsync(
        int customerId,
        int eftTransferId,
        CancellationToken cancellationToken = default);

    Task<List<PendingEftDto>> GetPendingAsync(
        CancellationToken cancellationToken = default);

    Task<EftDetailDto> ApproveAsync(
        int eftTransferId,
        int employeeId,
        CancellationToken cancellationToken = default);

    Task<EftDetailDto> RejectAsync(
        int eftTransferId,
        int employeeId,
        string reason,
        CancellationToken cancellationToken = default);
}

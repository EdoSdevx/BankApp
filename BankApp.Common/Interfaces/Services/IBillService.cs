using BankApp.BankApp.Common.Dtos.Bills;

namespace BankApp.BankApp.Common.Interfaces.Services;

public interface IBillService
{
    Task<Result<List<BillListDto>>> ListAsync(CancellationToken cancellationToken = default);
    Task<Result<BillSelectDto>> SelectAsync(int billId, CancellationToken cancellationToken = default);
    Task<Result> CreateAsync(BillCreateDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(BillUpdateDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int billId, CancellationToken cancellationToken = default);
    Task<Result> MarkPaidAsync(int billId, CancellationToken cancellationToken = default);
}

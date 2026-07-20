using BankApp.BankApp.Common.Dtos.Bills;

namespace BankApp.BankApp.Common.Interfaces.DataAccess;

public interface IBillDataAccess
{
    Task<List<BillListDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<BillSelectDto?> SelectAsync(int billId, CancellationToken cancellationToken = default);
    Task<int> InsertAsync(BillCreateDto bill, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(BillUpdateDto bill, CancellationToken cancellationToken = default);
    Task<int> DeleteAsync(int billId, CancellationToken cancellationToken = default);
    Task<int> MarkPaidAsync(int billId, CancellationToken cancellationToken = default);
}

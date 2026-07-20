using BankApp.BankApp.Common.Dtos.Accounts;
using BankApp.BankApp.Common.Dtos.Customer;

namespace BankApp.BankApp.Common.Interfaces.DataAccess;

public interface IAccountDataAccess
{
    Task<List<AccountListDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<AccountSelectDto?> SelectAsync(int accountId, CancellationToken cancellationToken = default);
    Task<int> InsertAsync(AccountCreateDto account, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(AccountUpdateDto account, CancellationToken cancellationToken = default);
    Task<int> DeleteAsync(int accountId, CancellationToken cancellationToken = default);
    Task TransferBetweenAsync(AccountTransferDto dto, CancellationToken cancellationToken = default);
}

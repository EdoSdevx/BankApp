using BankApp.BankApp.Common.Dtos.Accounts;
using BankApp.BankApp.Common.Dtos.Customer;

namespace BankApp.BankApp.Common.Interfaces.Services;

public interface IAccountService
{
    Task<Result<List<AccountListDto>>> ListAsync(CancellationToken cancellationToken = default);
    Task<Result<AccountSelectDto>> SelectAsync(int accountId, CancellationToken cancellationToken = default);
    Task<Result> CreateAsync(AccountCreateDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(AccountUpdateDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int accountId, CancellationToken cancellationToken = default);
    Task<Result> TransferBetweenAsync(AccountTransferDto dto, CancellationToken cancellationToken = default);
}

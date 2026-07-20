using BankApp.BankApp.Common.Dtos.Transactions;

namespace BankApp.BankApp.Common.Interfaces.Services;

public interface ITransactionService
{
    Task<Result<List<TransactionListDto>>> ListAsync(CancellationToken cancellationToken = default);
    Task<Result<TransactionSelectDto>> SelectAsync(int transactionId, CancellationToken cancellationToken = default);
    Task<Result> CreateAsync(TransactionCreateDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(TransactionUpdateDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int transactionId, CancellationToken cancellationToken = default);
}

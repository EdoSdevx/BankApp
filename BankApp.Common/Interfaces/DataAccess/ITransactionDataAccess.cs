using BankApp.BankApp.Common.Dtos.Transactions;

namespace BankApp.BankApp.Common.Interfaces.DataAccess;

public interface ITransactionDataAccess
{
    Task<List<TransactionListDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<TransactionSelectDto?> SelectAsync(int transactionId, CancellationToken cancellationToken = default);
    Task<int> InsertAsync(TransactionCreateDto transaction, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(TransactionUpdateDto transaction, CancellationToken cancellationToken = default);
    Task<int> DeleteAsync(int transactionId, CancellationToken cancellationToken = default);
}

using BankApp.BankApp.Common.Dtos.ExchangeRates;

namespace BankApp.BankApp.Common.Interfaces.DataAccess;

public interface IExchangeRateDataAccess
{
    Task<List<ExchangeRateListDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<ExchangeRateSelectDto?> SelectAsync(int rateId, CancellationToken cancellationToken = default);
    Task<int> InsertAsync(ExchangeRateCreateDto rate, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(ExchangeRateUpdateDto rate, CancellationToken cancellationToken = default);
    Task<int> DeleteAsync(int rateId, CancellationToken cancellationToken = default);
}

using BankApp.BankApp.Common.Dtos.Currencies;

namespace BankApp.BankApp.Common.Interfaces.DataAccess;

public interface ICurrencyDataAccess
{
    Task<List<CurrencyListDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<CurrencySelectDto?> SelectAsync(string currencyCode, CancellationToken cancellationToken = default);
    Task<int> InsertAsync(CurrencyCreateDto currency, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(CurrencyUpdateDto currency, CancellationToken cancellationToken = default);
    Task<int> DeleteAsync(string currencyCode, CancellationToken cancellationToken = default);
}

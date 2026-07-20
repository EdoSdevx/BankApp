using BankApp.BankApp.Common.Dtos.Currencies;

namespace BankApp.BankApp.Common.Interfaces.Services;

public interface ICurrencyService
{
    Task<Result<List<CurrencyListDto>>> ListAsync(CancellationToken cancellationToken = default);
    Task<Result<CurrencySelectDto>> SelectAsync(string currencyCode, CancellationToken cancellationToken = default);
    Task<Result> CreateAsync(CurrencyCreateDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(CurrencyUpdateDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(string currencyCode, CancellationToken cancellationToken = default);
}

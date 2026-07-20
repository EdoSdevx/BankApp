using BankApp.BankApp.Common.Dtos.ExchangeRates;

namespace BankApp.BankApp.Common.Interfaces.Services;

public interface IExchangeRateService
{
    Task<Result<List<ExchangeRateListDto>>> ListAsync(CancellationToken cancellationToken = default);
    Task<Result<ExchangeRateSelectDto>> SelectAsync(int rateId, CancellationToken cancellationToken = default);
    Task<Result> CreateAsync(ExchangeRateCreateDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(ExchangeRateUpdateDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int rateId, CancellationToken cancellationToken = default);
}

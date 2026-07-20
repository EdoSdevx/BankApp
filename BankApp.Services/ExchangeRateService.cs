using BankApp.BankApp.Common;
using BankApp.BankApp.Common.Dtos.ExchangeRates;
using BankApp.BankApp.Common.Interfaces.Services;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using BankApp.BankApp.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly IExchangeRateDataAccess _dataAccess;

    public ExchangeRateService(IExchangeRateDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public async Task<Result<List<ExchangeRateListDto>>> ListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var rates = await _dataAccess.ListAsync(cancellationToken);
            return Result<List<ExchangeRateListDto>>.Ok(rates);
        }
        catch (Exception ex)
        {
            return Result<List<ExchangeRateListDto>>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<ExchangeRateSelectDto>> SelectAsync(int rateId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (rateId <= 0)
            {
                return Result<ExchangeRateSelectDto>.Fail("Rate ID must be greater than zero.");
            }

            var rate = await _dataAccess.SelectAsync(rateId, cancellationToken);

            if (rate is null)
            {
                return Result<ExchangeRateSelectDto>.NotFound("Exchange rate not found.");
            }

            return Result<ExchangeRateSelectDto>.Ok(rate);
        }
        catch (Exception ex)
        {
            return Result<ExchangeRateSelectDto>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> CreateAsync(ExchangeRateCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = ValidateCreate(dto);
            if (validationResult is not null)
            {
                return validationResult;
            }

            dto.CurrencyCode = dto.CurrencyCode!.Trim().ToUpperInvariant();
            var rateId = await _dataAccess.InsertAsync(dto, cancellationToken);

            return Result.Ok("Exchange rate created successfully.");
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            return Result.Conflict("A record with this unique value already exists.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> UpdateAsync(ExchangeRateUpdateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = ValidateUpdate(dto);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var existing = await _dataAccess.SelectAsync(dto.RateId, cancellationToken);
            if (existing is null)
            {
                return Result.NotFound("Exchange rate not found.");
            }

            dto.CurrencyCode = dto.CurrencyCode!.Trim().ToUpperInvariant();

            await _dataAccess.UpdateAsync(dto, cancellationToken);

            return Result.Ok("Exchange rate updated successfully.");
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            return Result.Conflict("A record with this unique value already exists.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> DeleteAsync(int rateId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (rateId <= 0)
            {
                return Result.Fail("Rate ID must be greater than zero.");
            }

            var existing = await _dataAccess.SelectAsync(rateId, cancellationToken);
            if (existing is null)
            {
                return Result.NotFound("Exchange rate not found.");
            }

            await _dataAccess.DeleteAsync(rateId, cancellationToken);

            return Result.Ok("Exchange rate deleted successfully.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    private static Result? ValidateCreate(ExchangeRateCreateDto dto)
    {
        var failures = new List<ValidationFailure>();

        if (string.IsNullOrWhiteSpace(dto.CurrencyCode) || dto.CurrencyCode.Trim().Length != 3)
            failures.Add(new ValidationFailure(nameof(dto.CurrencyCode), "Currency code must be 3 characters."));

        if (dto.Rate <= 0)
            failures.Add(new ValidationFailure(nameof(dto.Rate), "Rate must be greater than zero."));

        if (string.IsNullOrWhiteSpace(dto.Source))
            failures.Add(new ValidationFailure(nameof(dto.Source), "Source is required."));

        return failures.Count > 0 ? Result.ValidationError(failures) : null;
    }

    private static Result? ValidateUpdate(ExchangeRateUpdateDto dto)
    {
        var failures = new List<ValidationFailure>();

        if (dto.RateId <= 0)
            failures.Add(new ValidationFailure(nameof(dto.RateId), "Rate ID must be greater than zero."));

        if (string.IsNullOrWhiteSpace(dto.CurrencyCode) || dto.CurrencyCode.Trim().Length != 3)
            failures.Add(new ValidationFailure(nameof(dto.CurrencyCode), "Currency code must be 3 characters."));

        if (dto.Rate <= 0)
            failures.Add(new ValidationFailure(nameof(dto.Rate), "Rate must be greater than zero."));

        if (string.IsNullOrWhiteSpace(dto.Source))
            failures.Add(new ValidationFailure(nameof(dto.Source), "Source is required."));

        return failures.Count > 0 ? Result.ValidationError(failures) : null;
    }
}

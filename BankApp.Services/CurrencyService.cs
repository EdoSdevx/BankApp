using BankApp.BankApp.Common;
using BankApp.BankApp.Common.Dtos.Currencies;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using BankApp.BankApp.Common.Interfaces.Services;
using BankApp.BankApp.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.Services;

public class CurrencyService : ICurrencyService
{
    private readonly ICurrencyDataAccess _dataAccess;

    public CurrencyService(ICurrencyDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public async Task<Result<List<CurrencyListDto>>> ListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var currencies = await _dataAccess.ListAsync(cancellationToken);
            return Result<List<CurrencyListDto>>.Ok(currencies);
        }
        catch (Exception ex)
        {
            return Result<List<CurrencyListDto>>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<CurrencySelectDto>> SelectAsync(string currencyCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = ValidateCurrencyCode(currencyCode);
            if (validationResult is not null)
            {
                return Result<CurrencySelectDto>.Fail(validationResult.Message);
            }

            var currency = await _dataAccess.SelectAsync(currencyCode.Trim().ToUpperInvariant(), cancellationToken);

            if (currency is null)
            {
                return Result<CurrencySelectDto>.NotFound("Currency not found.");
            }

            return Result<CurrencySelectDto>.Ok(currency);
        }
        catch (Exception ex)
        {
            return Result<CurrencySelectDto>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> CreateAsync(CurrencyCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = ValidateCreate(dto);
            if (validationResult is not null)
            {
                return validationResult;
            }

            dto.CurrencyCode = dto.CurrencyCode.Trim().ToUpperInvariant();
            await _dataAccess.InsertAsync(dto, cancellationToken);

            return Result.Ok("Currency created successfully.");
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            return Result.Conflict("A currency with this code already exists.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> UpdateAsync(CurrencyUpdateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = ValidateUpdate(dto);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var currencyCode = dto.CurrencyCode.Trim().ToUpperInvariant();
            var existing = await _dataAccess.SelectAsync(currencyCode, cancellationToken);
            if (existing is null)
            {
                return Result.NotFound("Currency not found.");
            }

            var merged = new CurrencyUpdateDto
            {
                CurrencyCode = currencyCode,
                CurrencyName = dto.CurrencyName ?? existing.CurrencyName
            };

            await _dataAccess.UpdateAsync(merged, cancellationToken);

            return Result.Ok("Currency updated successfully.");
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            return Result.Conflict("A currency with this code already exists.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> DeleteAsync(string currencyCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = ValidateCurrencyCode(currencyCode);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var normalizedCode = currencyCode.Trim().ToUpperInvariant();
            var existing = await _dataAccess.SelectAsync(normalizedCode, cancellationToken);
            if (existing is null)
            {
                return Result.NotFound("Currency not found.");
            }

            await _dataAccess.DeleteAsync(normalizedCode, cancellationToken);

            return Result.Ok("Currency deleted successfully.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    private static Result? ValidateCreate(CurrencyCreateDto dto)
    {
        var failures = new List<ValidationFailure>();

        if (string.IsNullOrWhiteSpace(dto.CurrencyCode) || dto.CurrencyCode.Trim().Length != 3)
            failures.Add(new ValidationFailure(nameof(dto.CurrencyCode), "Currency code must be 3 characters."));

        if (string.IsNullOrWhiteSpace(dto.CurrencyName))
            failures.Add(new ValidationFailure(nameof(dto.CurrencyName), "Currency name is required."));

        return failures.Count > 0 ? Result.ValidationError(failures) : null;
    }

    private static Result? ValidateUpdate(CurrencyUpdateDto dto)
    {
        var failures = new List<ValidationFailure>();

        if (string.IsNullOrWhiteSpace(dto.CurrencyCode) || dto.CurrencyCode.Trim().Length != 3)
            failures.Add(new ValidationFailure(nameof(dto.CurrencyCode), "Currency code must be 3 characters."));

        if (dto.CurrencyName is not null && string.IsNullOrWhiteSpace(dto.CurrencyName))
            failures.Add(new ValidationFailure(nameof(dto.CurrencyName), "Currency name cannot be empty."));

        return failures.Count > 0 ? Result.ValidationError(failures) : null;
    }

    private static Result? ValidateCurrencyCode(string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Trim().Length != 3)
            return Result.Fail("Currency code must be 3 characters.");

        return null;
    }
}

using BankApp.BankApp.Common;
using BankApp.BankApp.Common.Dtos.Transactions;
using BankApp.BankApp.Common.Interfaces.Services;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using BankApp.BankApp.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionDataAccess _dataAccess;

    public TransactionService(ITransactionDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public async Task<Result<List<TransactionListDto>>> ListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var transactions = await _dataAccess.ListAsync(cancellationToken);
            return Result<List<TransactionListDto>>.Ok(transactions);
        }
        catch (Exception ex)
        {
            return Result<List<TransactionListDto>>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<TransactionSelectDto>> SelectAsync(int transactionId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (transactionId <= 0)
            {
                return Result<TransactionSelectDto>.Fail("Transaction ID must be greater than zero.");
            }

            var transaction = await _dataAccess.SelectAsync(transactionId, cancellationToken);

            if (transaction is null)
            {
                return Result<TransactionSelectDto>.NotFound("Transaction not found.");
            }

            return Result<TransactionSelectDto>.Ok(transaction);
        }
        catch (Exception ex)
        {
            return Result<TransactionSelectDto>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> CreateAsync(TransactionCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = ValidateCreate(dto);
            if (validationResult is not null)
            {
                return validationResult;
            }

            dto.CurrencyCode = dto.CurrencyCode.Trim().ToUpperInvariant();
            var transactionId = await _dataAccess.InsertAsync(dto, cancellationToken);

            return Result.Ok("Transaction created successfully.");
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

    public async Task<Result> UpdateAsync(TransactionUpdateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = ValidateUpdate(dto);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var existing = await _dataAccess.SelectAsync(dto.TransactionId, cancellationToken);
            if (existing is null)
            {
                return Result.NotFound("Transaction not found.");
            }

            var merged = new TransactionUpdateDto
            {
                TransactionId = dto.TransactionId,
                AccountId = dto.AccountId ?? existing.AccountId,
                TransactionType = dto.TransactionType ?? existing.TransactionType,
                Amount = dto.Amount ?? existing.Amount,
                CurrencyCode = (dto.CurrencyCode ?? existing.CurrencyCode).Trim().ToUpperInvariant(),
                Description = dto.Description ?? existing.Description
            };

            await _dataAccess.UpdateAsync(merged, cancellationToken);

            return Result.Ok("Transaction updated successfully.");
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

    public async Task<Result> DeleteAsync(int transactionId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (transactionId <= 0)
            {
                return Result.Fail("Transaction ID must be greater than zero.");
            }

            var existing = await _dataAccess.SelectAsync(transactionId, cancellationToken);
            if (existing is null)
            {
                return Result.NotFound("Transaction not found.");
            }

            await _dataAccess.DeleteAsync(transactionId, cancellationToken);

            return Result.Ok("Transaction deleted successfully.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    private static Result? ValidateCreate(TransactionCreateDto dto)
    {
        var failures = new List<ValidationFailure>();

        if (dto.AccountId <= 0)
            failures.Add(new ValidationFailure(nameof(dto.AccountId), "Account ID must be greater than zero."));

        if (string.IsNullOrWhiteSpace(dto.TransactionType))
            failures.Add(new ValidationFailure(nameof(dto.TransactionType), "Transaction type is required."));

        if (dto.Amount <= 0)
            failures.Add(new ValidationFailure(nameof(dto.Amount), "Amount must be greater than zero."));

        if (string.IsNullOrWhiteSpace(dto.CurrencyCode) || dto.CurrencyCode.Trim().Length != 3)
            failures.Add(new ValidationFailure(nameof(dto.CurrencyCode), "Currency code must be 3 characters."));

        return failures.Count > 0 ? Result.ValidationError(failures) : null;
    }

    private static Result? ValidateUpdate(TransactionUpdateDto dto)
    {
        var failures = new List<ValidationFailure>();

        if (dto.TransactionId <= 0)
            failures.Add(new ValidationFailure(nameof(dto.TransactionId), "Transaction ID must be greater than zero."));

        if (dto.AccountId is <= 0)
            failures.Add(new ValidationFailure(nameof(dto.AccountId), "Account ID must be greater than zero."));

        if (dto.TransactionType is not null && string.IsNullOrWhiteSpace(dto.TransactionType))
            failures.Add(new ValidationFailure(nameof(dto.TransactionType), "Transaction type cannot be empty."));

        if (dto.Amount is <= 0)
            failures.Add(new ValidationFailure(nameof(dto.Amount), "Amount must be greater than zero."));

        if (dto.CurrencyCode is not null && (string.IsNullOrWhiteSpace(dto.CurrencyCode) || dto.CurrencyCode.Trim().Length != 3))
            failures.Add(new ValidationFailure(nameof(dto.CurrencyCode), "Currency code must be 3 characters."));

        return failures.Count > 0 ? Result.ValidationError(failures) : null;
    }
}

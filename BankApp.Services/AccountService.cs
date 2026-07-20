using BankApp.BankApp.Common;
using BankApp.BankApp.Common.Dtos.Accounts;
using BankApp.BankApp.Common.Dtos.Customer;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using BankApp.BankApp.Common.Interfaces.Services;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.Services;

public class AccountService : IAccountService
{
    private readonly IAccountDataAccess _dataAccess;

    public AccountService(IAccountDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public async Task<Result<List<AccountListDto>>> ListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var accounts = await _dataAccess.ListAsync(cancellationToken);
            return Result<List<AccountListDto>>.Ok(accounts);
        }
        catch (Exception ex)
        {
            return Result<List<AccountListDto>>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<AccountSelectDto>> SelectAsync(int accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (accountId <= 0)
            {
                return Result<AccountSelectDto>.Fail("Account ID must be greater than zero.");
            }

            var account = await _dataAccess.SelectAsync(accountId, cancellationToken);

            if (account is null)
            {
                return Result<AccountSelectDto>.NotFound("Account not found.");
            }

            return Result<AccountSelectDto>.Ok(account);
        }
        catch (Exception ex)
        {
            return Result<AccountSelectDto>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> CreateAsync(AccountCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = ValidateCreate(dto);
            if (validationResult is not null)
            {
                return validationResult;
            }

            dto.CurrencyCode = dto.CurrencyCode.Trim().ToUpperInvariant();
            var accountId = await _dataAccess.InsertAsync(dto, cancellationToken);

            return Result.Ok("Account created successfully.");
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

    public async Task<Result> UpdateAsync(AccountUpdateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = ValidateUpdate(dto);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var existing = await _dataAccess.SelectAsync(dto.AccountId, cancellationToken);
            if (existing is null)
            {
                return Result.NotFound("Account not found.");
            }

            var merged = new AccountUpdateDto
            {
                AccountId = dto.AccountId,
                CustomerId = dto.CustomerId ?? existing.CustomerId,
                BranchId = dto.BranchId ?? existing.BranchId,
                CurrencyCode = (dto.CurrencyCode ?? existing.CurrencyCode).Trim().ToUpperInvariant(),
                Balance = dto.Balance ?? existing.Balance
            };

            await _dataAccess.UpdateAsync(merged, cancellationToken);

            return Result.Ok("Account updated successfully.");
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

    public async Task<Result> DeleteAsync(int accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (accountId <= 0)
            {
                return Result.Fail("Account ID must be greater than zero.");
            }

            var existing = await _dataAccess.SelectAsync(accountId, cancellationToken);
            if (existing is null)
            {
                return Result.NotFound("Account not found.");
            }

            await _dataAccess.DeleteAsync(accountId, cancellationToken);

            return Result.Ok("Account deleted successfully.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> TransferBetweenAsync(AccountTransferDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (dto.SourceAccountId <= 0)
                return Result.Fail("Source account ID must be greater than zero.");

            if (dto.TargetAccountId <= 0)
                return Result.Fail("Target account ID must be greater than zero.");

            if (dto.SourceAccountId == dto.TargetAccountId)
                return Result.Fail("Source and target accounts must be different.");

            if (dto.Amount <= 0)
                return Result.Fail("Amount must be greater than zero.");

            await _dataAccess.TransferBetweenAsync(dto, cancellationToken);

            return Result.Ok("Transfer completed successfully.");
        }
        catch (SqlException ex)
        {
            return Result.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    private static Result? ValidateCreate(AccountCreateDto dto)
    {
        var failures = new List<ValidationFailure>();

        if (dto.CustomerId <= 0)
            failures.Add(new ValidationFailure(nameof(dto.CustomerId), "Customer ID must be greater than zero."));

        if (dto.BranchId <= 0)
            failures.Add(new ValidationFailure(nameof(dto.BranchId), "Branch ID must be greater than zero."));

        if (string.IsNullOrWhiteSpace(dto.CurrencyCode) || dto.CurrencyCode.Trim().Length != 3)
            failures.Add(new ValidationFailure(nameof(dto.CurrencyCode), "Currency code must be 3 characters."));

        if (dto.Balance < 0)
            failures.Add(new ValidationFailure(nameof(dto.Balance), "Balance cannot be negative."));

        return failures.Count > 0 ? Result.ValidationError(failures) : null;
    }

    private static Result? ValidateUpdate(AccountUpdateDto dto)
    {
        var failures = new List<ValidationFailure>();

        if (dto.AccountId <= 0)
            failures.Add(new ValidationFailure(nameof(dto.AccountId), "Account ID must be greater than zero."));

        if (dto.CustomerId is <= 0)
            failures.Add(new ValidationFailure(nameof(dto.CustomerId), "Customer ID must be greater than zero."));

        if (dto.BranchId is <= 0)
            failures.Add(new ValidationFailure(nameof(dto.BranchId), "Branch ID must be greater than zero."));

        if (dto.CurrencyCode is not null && (string.IsNullOrWhiteSpace(dto.CurrencyCode) || dto.CurrencyCode.Trim().Length != 3))
            failures.Add(new ValidationFailure(nameof(dto.CurrencyCode), "Currency code must be 3 characters."));

        if (dto.Balance is < 0)
            failures.Add(new ValidationFailure(nameof(dto.Balance), "Balance cannot be negative."));

        return failures.Count > 0 ? Result.ValidationError(failures) : null;
    }
}

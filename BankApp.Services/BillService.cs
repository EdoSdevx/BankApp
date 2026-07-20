using BankApp.BankApp.Common;
using BankApp.BankApp.Common.Dtos.Bills;
using BankApp.BankApp.Common.Interfaces.Services;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using BankApp.BankApp.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.Services;

public class BillService : IBillService
{
    private readonly IBillDataAccess _dataAccess;

    public BillService(IBillDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public async Task<Result<List<BillListDto>>> ListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var bills = await _dataAccess.ListAsync(cancellationToken);
            return Result<List<BillListDto>>.Ok(bills);
        }
        catch (Exception ex)
        {
            return Result<List<BillListDto>>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<BillSelectDto>> SelectAsync(int billId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (billId <= 0)
            {
                return Result<BillSelectDto>.Fail("Bill ID must be greater than zero.");
            }

            var bill = await _dataAccess.SelectAsync(billId, cancellationToken);

            if (bill is null)
            {
                return Result<BillSelectDto>.NotFound("Bill not found.");
            }

            return Result<BillSelectDto>.Ok(bill);
        }
        catch (Exception ex)
        {
            return Result<BillSelectDto>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> CreateAsync(BillCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = ValidateCreate(dto);
            if (validationResult is not null)
            {
                return validationResult;
            }

            dto.CurrencyCode = NormalizeOptionalCurrencyCode(dto.CurrencyCode);
            var billId = await _dataAccess.InsertAsync(dto, cancellationToken);

            return Result.Ok("Bill created successfully.");
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

    public async Task<Result> UpdateAsync(BillUpdateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = ValidateUpdate(dto);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var existing = await _dataAccess.SelectAsync(dto.BillId, cancellationToken);
            if (existing is null)
            {
                return Result.NotFound("Bill not found.");
            }

            var merged = new BillUpdateDto
            {
                BillId = dto.BillId,
                CustomerId = dto.CustomerId ?? existing.CustomerId,
                BillType = dto.BillType ?? existing.BillType,
                Amount = dto.Amount ?? existing.Amount,
                CurrencyCode = NormalizeOptionalCurrencyCode(dto.CurrencyCode ?? existing.CurrencyCode),
                DueDate = dto.DueDate ?? existing.DueDate,
                IsPaid = dto.IsPaid ?? existing.IsPaid,
                PaidDate = dto.PaidDate ?? existing.PaidDate
            };

            await _dataAccess.UpdateAsync(merged, cancellationToken);

            return Result.Ok("Bill updated successfully.");
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

    public async Task<Result> MarkPaidAsync(int billId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (billId <= 0)
            {
                return Result.Fail("Bill ID must be greater than zero.");
            }

            var existing = await _dataAccess.SelectAsync(billId, cancellationToken);
            if (existing is null)
            {
                return Result.NotFound("Bill not found.");
            }

            if (existing.IsPaid)
            {
                return Result.Fail("Bill is already marked as paid.");
            }

            await _dataAccess.MarkPaidAsync(billId, cancellationToken);

            return Result.Ok("Bill marked as paid.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> DeleteAsync(int billId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (billId <= 0)
            {
                return Result.Fail("Bill ID must be greater than zero.");
            }

            var existing = await _dataAccess.SelectAsync(billId, cancellationToken);
            if (existing is null)
            {
                return Result.NotFound("Bill not found.");
            }

            await _dataAccess.DeleteAsync(billId, cancellationToken);

            return Result.Ok("Bill deleted successfully.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    private static Result? ValidateCreate(BillCreateDto dto)
    {
        var failures = new List<ValidationFailure>();

        if (dto.CustomerId <= 0)
            failures.Add(new ValidationFailure(nameof(dto.CustomerId), "Customer ID must be greater than zero."));

        if (string.IsNullOrWhiteSpace(dto.BillType))
            failures.Add(new ValidationFailure(nameof(dto.BillType), "Bill type is required."));

        if (dto.Amount <= 0)
            failures.Add(new ValidationFailure(nameof(dto.Amount), "Amount must be greater than zero."));

        if (dto.CurrencyCode is not null && (string.IsNullOrWhiteSpace(dto.CurrencyCode) || dto.CurrencyCode.Trim().Length != 3))
            failures.Add(new ValidationFailure(nameof(dto.CurrencyCode), "Currency code must be 3 characters."));

        if (dto.DueDate == default)
            failures.Add(new ValidationFailure(nameof(dto.DueDate), "Due date is required."));

        if (!dto.IsPaid && dto.PaidDate is not null)
            failures.Add(new ValidationFailure(nameof(dto.PaidDate), "Paid date cannot be set when bill is unpaid."));

        return failures.Count > 0 ? Result.ValidationError(failures) : null;
    }

    private static Result? ValidateUpdate(BillUpdateDto dto)
    {
        var failures = new List<ValidationFailure>();

        if (dto.BillId <= 0)
            failures.Add(new ValidationFailure(nameof(dto.BillId), "Bill ID must be greater than zero."));

        if (dto.CustomerId is <= 0)
            failures.Add(new ValidationFailure(nameof(dto.CustomerId), "Customer ID must be greater than zero."));

        if (dto.BillType is not null && string.IsNullOrWhiteSpace(dto.BillType))
            failures.Add(new ValidationFailure(nameof(dto.BillType), "Bill type cannot be empty."));

        if (dto.Amount is <= 0)
            failures.Add(new ValidationFailure(nameof(dto.Amount), "Amount must be greater than zero."));

        if (dto.CurrencyCode is not null && (string.IsNullOrWhiteSpace(dto.CurrencyCode) || dto.CurrencyCode.Trim().Length != 3))
            failures.Add(new ValidationFailure(nameof(dto.CurrencyCode), "Currency code must be 3 characters."));

        if (dto.DueDate == default)
            failures.Add(new ValidationFailure(nameof(dto.DueDate), "Due date is invalid."));

        if (dto.IsPaid == false && dto.PaidDate is not null)
            failures.Add(new ValidationFailure(nameof(dto.PaidDate), "Paid date cannot be set when bill is unpaid."));

        return failures.Count > 0 ? Result.ValidationError(failures) : null;
    }

    private static string? NormalizeOptionalCurrencyCode(string? currencyCode)
    {
        return string.IsNullOrWhiteSpace(currencyCode)
            ? null
            : currencyCode.Trim().ToUpperInvariant();
    }
}

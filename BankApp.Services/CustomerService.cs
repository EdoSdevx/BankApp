using BankApp.BankApp.Common;
using BankApp.BankApp.Common.Dtos.Customers;
using BankApp.BankApp.Common.Helpers;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using BankApp.BankApp.Common.Interfaces.Services;
using BankApp.BankApp.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerDataAccess _dataAccess;

    public CustomerService(ICustomerDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public async Task<Result<List<CustomerListDto>>> ListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var customers = await _dataAccess.ListAsync(cancellationToken);
            return Result<List<CustomerListDto>>.Ok(customers);
        }
        catch (Exception ex)
        {
            return Result<List<CustomerListDto>>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<CustomerSelectDto>> SelectAsync(int customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (customerId <= 0)
            {
                return Result<CustomerSelectDto>.Fail("Customer ID must be greater than zero.");
            }

            var customer = await _dataAccess.SelectAsync(customerId, cancellationToken);

            if (customer is null)
            {
                return Result<CustomerSelectDto>.NotFound("Customer not found.");
            }

            return Result<CustomerSelectDto>.Ok(customer);
        }
        catch (Exception ex)
        {
            return Result<CustomerSelectDto>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> CreateAsync(CustomerCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = ValidateCreate(dto);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var passwordHash = PasswordHasher.Hash(dto.Password);

            var customerId = await _dataAccess.InsertAsync(dto, passwordHash, isActive: true, cancellationToken);

            return Result.Ok("Customer created successfully.");
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            return Result.Conflict($"Unique constraint violation: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> UpdateAsync(CustomerUpdateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            NormalizeUpdateDto(dto);

            var validationResult = ValidateUpdate(dto);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var existing = await _dataAccess.SelectAsync(dto.CustomerId, cancellationToken);
            if (existing is null)
            {
                return Result.NotFound("Customer not found.");
            }

            var merged = new CustomerUpdateDto
            {
                CustomerId = dto.CustomerId,
                FirstName = dto.FirstName ?? existing.FirstName,
                LastName = dto.LastName ?? existing.LastName,
                Email = dto.Email ?? existing.Email,
                Phone = dto.Phone ?? existing.Phone,
                Address = dto.Address ?? existing.Address,
                IsActive = dto.IsActive ?? existing.IsActive
            };

            string? passwordHash = dto.Password is not null
                ? PasswordHasher.Hash(dto.Password)
                : existing.PasswordHash;

            await _dataAccess.UpdateAsync(merged, passwordHash, cancellationToken);

            return Result.Ok("Customer updated successfully.");
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            return Result.Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> DeleteAsync(int customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (customerId <= 0)
            {
                return Result.Fail("Customer ID must be greater than zero.");
            }

            var existing = await _dataAccess.SelectAsync(customerId, cancellationToken);
            if (existing is null)
            {
                return Result.NotFound("Customer not found.");
            }

            await _dataAccess.DeleteAsync(customerId, cancellationToken);

            return Result.Ok("Customer deleted successfully.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    private static void NormalizeUpdateDto(CustomerUpdateDto dto)
    {
        dto.FirstName = NormalizeOptionalText(dto.FirstName);
        dto.LastName = NormalizeOptionalText(dto.LastName);
        dto.Email = NormalizeOptionalText(dto.Email);
        dto.Phone = NormalizeOptionalText(dto.Phone);
        dto.Address = NormalizeOptionalText(dto.Address);
        dto.Password = NormalizeOptionalText(dto.Password);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value;
    }

    private static Result? ValidateCreate(CustomerCreateDto dto)
    {
        var failures = new List<ValidationFailure>();

        if (string.IsNullOrWhiteSpace(dto.FirstName))
            failures.Add(new ValidationFailure(nameof(dto.FirstName), "First name is required."));

        if (string.IsNullOrWhiteSpace(dto.LastName))
            failures.Add(new ValidationFailure(nameof(dto.LastName), "Last name is required."));

        if (string.IsNullOrWhiteSpace(dto.Email))
            failures.Add(new ValidationFailure(nameof(dto.Email), "Email is required."));
        else if (!dto.Email.Contains('@'))
            failures.Add(new ValidationFailure(nameof(dto.Email), "Email must contain '@'."));

        if (string.IsNullOrWhiteSpace(dto.Address))
            failures.Add(new ValidationFailure(nameof(dto.Address), "Address is required."));

        if (string.IsNullOrWhiteSpace(dto.Password))
            failures.Add(new ValidationFailure(nameof(dto.Password), "Password is required."));
        else if (dto.Password.Length < 6)
            failures.Add(new ValidationFailure(nameof(dto.Password), "Password must be at least 6 characters."));

        return failures.Count > 0 ? Result.ValidationError(failures) : null;
    }

    private static Result? ValidateUpdate(CustomerUpdateDto dto)
    {
        var failures = new List<ValidationFailure>();

        if (dto.CustomerId <= 0)
            failures.Add(new ValidationFailure(nameof(dto.CustomerId), "Customer ID must be greater than zero."));

        if (dto.FirstName is not null && string.IsNullOrWhiteSpace(dto.FirstName))
            failures.Add(new ValidationFailure(nameof(dto.FirstName), "First name cannot be empty."));

        if (dto.LastName is not null && string.IsNullOrWhiteSpace(dto.LastName))
            failures.Add(new ValidationFailure(nameof(dto.LastName), "Last name cannot be empty."));

        if (dto.Email is not null)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                failures.Add(new ValidationFailure(nameof(dto.Email), "Email cannot be empty."));
            else if (!dto.Email.Contains('@'))
                failures.Add(new ValidationFailure(nameof(dto.Email), "Email must contain '@'."));
        }

        if (dto.Address is not null && string.IsNullOrWhiteSpace(dto.Address))
            failures.Add(new ValidationFailure(nameof(dto.Address), "Address cannot be empty."));

        return failures.Count > 0 ? Result.ValidationError(failures) : null;
    }
}

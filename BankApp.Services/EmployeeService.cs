using BankApp.BankApp.Common;
using BankApp.BankApp.Common.Dtos.Employees;
using BankApp.BankApp.Common.Enums;
using BankApp.BankApp.Common.Helpers;
using BankApp.BankApp.Common.Interfaces.Services;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using BankApp.BankApp.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeDataAccess _dataAccess;

    public EmployeeService(IEmployeeDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public async Task<Result<List<EmployeeListDto>>> ListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var employees = await _dataAccess.ListAsync(cancellationToken);
            return Result<List<EmployeeListDto>>.Ok(employees);
        }
        catch (Exception ex)
        {
            return Result<List<EmployeeListDto>>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<EmployeeSelectDto>> SelectAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (employeeId <= 0)
            {
                return Result<EmployeeSelectDto>.Fail("Employee ID must be greater than zero.");
            }

            var employee = await _dataAccess.SelectAsync(employeeId, cancellationToken);

            if (employee is null)
            {
                return Result<EmployeeSelectDto>.NotFound("Employee not found.");
            }

            return Result<EmployeeSelectDto>.Ok(employee);
        }
        catch (Exception ex)
        {
            return Result<EmployeeSelectDto>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> CreateAsync(EmployeeCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = ValidateCreate(dto);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var passwordHash = PasswordHasher.Hash(dto.Password);

            var employeeId = await _dataAccess.InsertAsync(dto, passwordHash, cancellationToken);

            return Result.Ok("Employee created successfully.");
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            return Result.Conflict("An employee with this email already exists.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> UpdateAsync(EmployeeUpdateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = ValidateUpdate(dto);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var existing = await _dataAccess.SelectAsync(dto.EmployeeId, cancellationToken);
            if (existing is null)
            {
                return Result.NotFound("Employee not found.");
            }

            var merged = new EmployeeUpdateDto
            {
                EmployeeId = dto.EmployeeId,
                BranchId = dto.BranchId > 0 ? dto.BranchId : existing.BranchId,
                RoleId = dto.RoleId > 0 ? dto.RoleId : existing.RoleId,
                FirstName = dto.FirstName ?? existing.FirstName,
                LastName = dto.LastName ?? existing.LastName,
                Email = dto.Email ?? existing.Email,
                Phone = dto.Phone ?? existing.Phone,
                AuthRole = dto.AuthRole ?? existing.AuthRole
            };

            string? passwordHash = dto.Password is not null
                ? PasswordHasher.Hash(dto.Password)
                : existing.PasswordHash;

            await _dataAccess.UpdateAsync(merged, passwordHash, cancellationToken);

            return Result.Ok("Employee updated successfully.");
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            return Result.Conflict("An employee with this email already exists.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> DeleteAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (employeeId <= 0)
            {
                return Result.Fail("Employee ID must be greater than zero.");
            }

            var existing = await _dataAccess.SelectAsync(employeeId, cancellationToken);
            if (existing is null)
            {
                return Result.NotFound("Employee not found.");
            }

            await _dataAccess.DeleteAsync(employeeId, cancellationToken);

            return Result.Ok("Employee deleted successfully.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    private static Result? ValidateCreate(EmployeeCreateDto dto)
    {
        var failures = new List<ValidationFailure>();

        if (dto.BranchId <= 0)
            failures.Add(new ValidationFailure(nameof(dto.BranchId), "Branch ID must be greater than zero."));

        if (dto.RoleId <= 0)
            failures.Add(new ValidationFailure(nameof(dto.RoleId), "Role ID must be greater than zero."));

        if (string.IsNullOrWhiteSpace(dto.FirstName))
            failures.Add(new ValidationFailure(nameof(dto.FirstName), "First name is required."));

        if (string.IsNullOrWhiteSpace(dto.LastName))
            failures.Add(new ValidationFailure(nameof(dto.LastName), "Last name is required."));

        if (string.IsNullOrWhiteSpace(dto.Email))
            failures.Add(new ValidationFailure(nameof(dto.Email), "Email is required."));
        else if (!dto.Email.Contains('@'))
            failures.Add(new ValidationFailure(nameof(dto.Email), "Email must contain '@'."));

        if (string.IsNullOrWhiteSpace(dto.Phone))
            failures.Add(new ValidationFailure(nameof(dto.Phone), "Phone is required."));

        if (string.IsNullOrWhiteSpace(dto.Password))
            failures.Add(new ValidationFailure(nameof(dto.Password), "Password is required."));
        else if (dto.Password.Length < 6)
            failures.Add(new ValidationFailure(nameof(dto.Password), "Password must be at least 6 characters."));

        if (!IsValidEmployeeAuthRole(dto.AuthRole))
            failures.Add(new ValidationFailure(nameof(dto.AuthRole), "Auth role must be Employee or Admin."));

        return failures.Count > 0 ? Result.ValidationError(failures) : null;
    }

    private static Result? ValidateUpdate(EmployeeUpdateDto dto)
    {
        var failures = new List<ValidationFailure>();

        if (dto.EmployeeId <= 0)
            failures.Add(new ValidationFailure(nameof(dto.EmployeeId), "Employee ID must be greater than zero."));

        if (dto.BranchId is <= 0)
            failures.Add(new ValidationFailure(nameof(dto.BranchId), "Branch ID must be greater than zero."));

        if (dto.RoleId is <= 0)
            failures.Add(new ValidationFailure(nameof(dto.RoleId), "Role ID must be greater than zero."));

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

        if (dto.Phone is not null && string.IsNullOrWhiteSpace(dto.Phone))
            failures.Add(new ValidationFailure(nameof(dto.Phone), "Phone cannot be empty."));

        if (dto.AuthRole is not null && !IsValidEmployeeAuthRole(dto.AuthRole.Value))
            failures.Add(new ValidationFailure(nameof(dto.AuthRole), "Auth role must be Employee or Admin."));

        return failures.Count > 0 ? Result.ValidationError(failures) : null;
    }

    private static bool IsValidEmployeeAuthRole(AppRole authRole)
    {
        return authRole is AppRole.Employee or AppRole.Admin;
    }
}

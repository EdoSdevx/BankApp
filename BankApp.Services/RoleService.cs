using BankApp.BankApp.Common;
using BankApp.BankApp.Common.Dtos.Roles;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using BankApp.BankApp.Common.Interfaces.Services;
using BankApp.BankApp.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.Services;

public class RoleService : IRoleService
{
    private readonly IRoleDataAccess _dataAccess;

    public RoleService(IRoleDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public async Task<Result<List<RoleListDto>>> ListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var roles = await _dataAccess.ListAsync(cancellationToken);
            return Result<List<RoleListDto>>.Ok(roles);
        }
        catch (Exception ex)
        {
            return Result<List<RoleListDto>>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<RoleSelectDto>> SelectAsync(int roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (roleId <= 0)
            {
                return Result<RoleSelectDto>.Fail("Role ID must be greater than zero.");
            }

            var role = await _dataAccess.SelectAsync(roleId, cancellationToken);

            if (role is null)
            {
                return Result<RoleSelectDto>.NotFound("Role not found.");
            }

            return Result<RoleSelectDto>.Ok(role);
        }
        catch (Exception ex)
        {
            return Result<RoleSelectDto>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> CreateAsync(RoleCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = ValidateCreate(dto);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var roleId = await _dataAccess.InsertAsync(dto, cancellationToken);

            return Result.Ok("Role created successfully.");
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            return Result.Conflict("A role with this name already exists.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> UpdateAsync(RoleUpdateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = ValidateUpdate(dto);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var existing = await _dataAccess.SelectAsync(dto.RoleId, cancellationToken);
            if (existing is null)
            {
                return Result.NotFound("Role not found.");
            }

            var merged = new RoleUpdateDto
            {
                RoleId = dto.RoleId,
                RoleName = dto.RoleName ?? existing.RoleName,
                Description = dto.Description ?? existing.Description
            };

            await _dataAccess.UpdateAsync(merged, cancellationToken);

            return Result.Ok("Role updated successfully.");
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            return Result.Conflict("A role with this name already exists.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> DeleteAsync(int roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (roleId <= 0)
            {
                return Result.Fail("Role ID must be greater than zero.");
            }

            var existing = await _dataAccess.SelectAsync(roleId, cancellationToken);
            if (existing is null)
            {
                return Result.NotFound("Role not found.");
            }

            await _dataAccess.DeleteAsync(roleId, cancellationToken);

            return Result.Ok("Role deleted successfully.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    private static Result? ValidateCreate(RoleCreateDto dto)
    {
        var failures = new List<ValidationFailure>();

        if (string.IsNullOrWhiteSpace(dto.RoleName))
            failures.Add(new ValidationFailure(nameof(dto.RoleName), "Role name is required."));

        return failures.Count > 0 ? Result.ValidationError(failures) : null;
    }

    private static Result? ValidateUpdate(RoleUpdateDto dto)
    {
        var failures = new List<ValidationFailure>();

        if (dto.RoleId <= 0)
            failures.Add(new ValidationFailure(nameof(dto.RoleId), "Role ID must be greater than zero."));

        if (dto.RoleName is not null && string.IsNullOrWhiteSpace(dto.RoleName))
            failures.Add(new ValidationFailure(nameof(dto.RoleName), "Role name cannot be empty."));

        return failures.Count > 0 ? Result.ValidationError(failures) : null;
    }
}

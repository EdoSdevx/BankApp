using BankApp.BankApp.Common;
using BankApp.BankApp.Common.Dtos.Branches;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using BankApp.BankApp.Common.Interfaces.Services;
using BankApp.BankApp.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.Services;

public class BranchService : IBranchService
{
    private readonly IBranchDataAccess _dataAccess;

    public BranchService(IBranchDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public async Task<Result<List<BranchListDto>>> ListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var branches = await _dataAccess.ListAsync(cancellationToken);
            return Result<List<BranchListDto>>.Ok(branches);
        }
        catch (Exception ex)
        {
            return Result<List<BranchListDto>>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<BranchSelectDto>> SelectAsync(int branchId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (branchId <= 0)
            {
                return Result<BranchSelectDto>.Fail("Branch ID must be greater than zero.");
            }

            var branch = await _dataAccess.SelectAsync(branchId, cancellationToken);

            if (branch is null)
            {
                return Result<BranchSelectDto>.NotFound("Branch not found.");
            }

            return Result<BranchSelectDto>.Ok(branch);
        }
        catch (Exception ex)
        {
            return Result<BranchSelectDto>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> CreateAsync(BranchCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = ValidateCreate(dto);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var branchId = await _dataAccess.InsertAsync(dto, cancellationToken);

            return Result.Ok("Branch created successfully.");
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            return Result.Conflict("A branch with this code or name already exists.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> UpdateAsync(BranchUpdateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = ValidateUpdate(dto);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var existing = await _dataAccess.SelectAsync(dto.BranchId, cancellationToken);
            if (existing is null)
            {
                return Result.NotFound("Branch not found.");
            }

            var merged = new BranchUpdateDto
            {
                BranchId = dto.BranchId,
                BranchName = dto.BranchName ?? existing.BranchName,
                BranchCode = dto.BranchCode ?? existing.BranchCode,
                City = dto.City ?? existing.City,
                Address = dto.Address ?? existing.Address
            };

            await _dataAccess.UpdateAsync(merged, cancellationToken);

            return Result.Ok("Branch updated successfully.");
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            return Result.Conflict("A branch with this code or name already exists.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> DeleteAsync(int branchId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (branchId <= 0)
            {
                return Result.Fail("Branch ID must be greater than zero.");
            }

            var existing = await _dataAccess.SelectAsync(branchId, cancellationToken);
            if (existing is null)
            {
                return Result.NotFound("Branch not found.");
            }

            await _dataAccess.DeleteAsync(branchId, cancellationToken);

            return Result.Ok("Branch deleted successfully.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    private static Result? ValidateCreate(BranchCreateDto dto)
    {
        var failures = new List<ValidationFailure>();

        if (string.IsNullOrWhiteSpace(dto.BranchName))
            failures.Add(new ValidationFailure(nameof(dto.BranchName), "Branch name is required."));

        if (string.IsNullOrWhiteSpace(dto.BranchCode))
            failures.Add(new ValidationFailure(nameof(dto.BranchCode), "Branch code is required."));

        if (string.IsNullOrWhiteSpace(dto.City))
            failures.Add(new ValidationFailure(nameof(dto.City), "City is required."));

        if (string.IsNullOrWhiteSpace(dto.Address))
            failures.Add(new ValidationFailure(nameof(dto.Address), "Address is required."));

        return failures.Count > 0 ? Result.ValidationError(failures) : null;
    }

    private static Result? ValidateUpdate(BranchUpdateDto dto)
    {
        var failures = new List<ValidationFailure>();

        if (dto.BranchId <= 0)
            failures.Add(new ValidationFailure(nameof(dto.BranchId), "Branch ID must be greater than zero."));

        if (dto.BranchName is not null && string.IsNullOrWhiteSpace(dto.BranchName))
            failures.Add(new ValidationFailure(nameof(dto.BranchName), "Branch name cannot be empty."));

        if (dto.BranchCode is not null && string.IsNullOrWhiteSpace(dto.BranchCode))
            failures.Add(new ValidationFailure(nameof(dto.BranchCode), "Branch code cannot be empty."));

        if (dto.City is not null && string.IsNullOrWhiteSpace(dto.City))
            failures.Add(new ValidationFailure(nameof(dto.City), "City cannot be empty."));

        if (dto.Address is not null && string.IsNullOrWhiteSpace(dto.Address))
            failures.Add(new ValidationFailure(nameof(dto.Address), "Address cannot be empty."));

        return failures.Count > 0 ? Result.ValidationError(failures) : null;
    }
}
